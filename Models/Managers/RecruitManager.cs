using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using zModels = NewSprt.Data.Zarnica.Models;

namespace NewSprt.Models.Managers
{
    public class RecruitManager
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;

        public RecruitManager(ZarnicaDbContext zarnicaDb, AppDbContext appDb)
        {
            _zarnicaDb = zarnicaDb;
            _appDb = appDb;
        }

        public async Task<bool> SynchronizationOfDatabases()
        {
            try
            {
                var currentConscriptionsPeriod =
                    await _appDb.ConscriptionPeriods.FirstOrDefaultAsync(m => !m.IsArchive);

                var appCount = _appDb.Recruits.Count(m => m.ConscriptionPeriodId == currentConscriptionsPeriod.Id);
                var zarnicaCount = _zarnicaDb.Recruits.Count();
                if (zarnicaCount == appCount)
                {
                    return true;
                }

                var appRecruits = await _appDb.Recruits
                    .Where(m => m.ConscriptionPeriodId == currentConscriptionsPeriod.Id)
                    .Select(m => m.RecruitId).ToListAsync();
                var zarnicaRecruits = await _zarnicaDb.Recruits.Where(m => !appRecruits.Contains(m.Id))
                    .Select(m => new
                    {
                        m.Id,
                        m.Code,
                        m.DelivaryDate,
                        m.LastName,
                        m.FirstName,
                        m.Patronymic,
                        m.MilitaryComissariatId
                    }).ToListAsync();

                var newAppRecruits = zarnicaRecruits
                    .Select(zarnicaRecruit => new Recruit
                    {
                        RecruitId = zarnicaRecruit.Id,
                        ConscriptionPeriodId = currentConscriptionsPeriod.Id,
                        DeliveryDate = zarnicaRecruit.DelivaryDate,
                        UniqueRecruitNumber = zarnicaRecruit.Code,
                        LastName = zarnicaRecruit.LastName,
                        FirstName = zarnicaRecruit.FirstName,
                        Patronymic = zarnicaRecruit.Patronymic,
                        MilitaryComissariatCode = zarnicaRecruit.MilitaryComissariatId
                    }).ToList();
                await SynchrinizationACovidResultNumber(newAppRecruits);
                await _appDb.Recruits.AddRangeAsync(newAppRecruits);
                await _appDb.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SynchrinizationACovidResultNumber(List<Recruit> recruits)
        {
            try
            {
                var updateAdditionalDatas = new List<zModels.AdditionalData>();
                var recruitIds = recruits.Select(m => m.RecruitId);
                var relatives = await _zarnicaDb.Relatives.Where(m => recruitIds.Contains(m.RecruitId)).ToListAsync();
                var additionalDatas = await _zarnicaDb.AdditionalDatas.Where(m => recruitIds.Contains(m.Id)).ToListAsync();
                foreach (var recruit in recruits)
                {
                    var relative = relatives.FirstOrDefault(m =>
                        m.RecruitId == recruit.RecruitId && m.RelativeType == zModels.Relative.TempRelative);
                    if (relative == null) continue;
                
                    var additionalData = additionalDatas.FirstOrDefault(m => m.Id == recruit.RecruitId);
                    if (additionalData == null || !string.IsNullOrEmpty(additionalData.TestNum)) continue;
                    additionalData.TestDate = recruit.DeliveryDate;
                    additionalData.TestNum = relative.Fio;
                    updateAdditionalDatas.Add(additionalData);
                }

                _zarnicaDb.AdditionalDatas.UpdateRange(updateAdditionalDatas);
                await _zarnicaDb.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}