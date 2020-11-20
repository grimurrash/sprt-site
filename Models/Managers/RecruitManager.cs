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

        public RecruitManager(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
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

                var appCount =
                    await _appDb.Recruits.CountAsync(m => m.ConscriptionPeriodId == currentConscriptionsPeriod.Id);
                var zarnicaCount = await _zarnicaDb.Recruits.CountAsync();
                if (zarnicaCount < 5) return true;
                if (zarnicaCount > appCount)
                {
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
                            m.MilitaryComissariatId,
                            m.BirthDate
                        }).ToListAsync();

                    var newAppRecruits = zarnicaRecruits
                        .Select(zarnicaRecruit => new Recruit
                        {
                            RecruitId = zarnicaRecruit.Id,
                            ConscriptionPeriodId = currentConscriptionsPeriod.Id,
                            DeliveryDate = zarnicaRecruit.DelivaryDate,
                            UniqueRecruitNumber = zarnicaRecruit.Code,
                            DactyloscopyStatusId = 1,
                            LastName = zarnicaRecruit.LastName,
                            FirstName = zarnicaRecruit.FirstName,
                            Patronymic = zarnicaRecruit.Patronymic,
                            BirthDate = zarnicaRecruit.BirthDate,
                            MilitaryComissariatCode = zarnicaRecruit.MilitaryComissariatId
                        }).ToList();
                    await _appDb.Recruits.AddRangeAsync(newAppRecruits);
                    await _appDb.SaveChangesAsync();
                }
                else if (zarnicaCount < appCount)
                {
                    var zarnicaRecruits = await _zarnicaDb.Recruits.Select(m => new
                    {
                        m.Id,
                        m.Code
                    }).ToListAsync();
                    var appRecruits = await _appDb.Recruits
                        .Where(m => m.ConscriptionPeriodId == currentConscriptionsPeriod.Id &&
                                    !zarnicaRecruits.Select(z => z.Id).Contains(m.RecruitId) &&
                                    !zarnicaRecruits.Select(z => z.Code).Contains(m.UniqueRecruitNumber)
                        ).ToListAsync();
                    _appDb.Recruits.RemoveRange(appRecruits);
                    await _appDb.SaveChangesAsync();
                }

                await SynchrinizationACovidResultNumber();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SynchrinizationACovidResultNumber()
        {
            try
            {
                var updateAdditionalDatas = new List<zModels.AdditionalData>();
                var additionalDatas = await _zarnicaDb.AdditionalDatas.Where(m => string.IsNullOrEmpty(m.TestNum))
                    .ToListAsync();
                if (additionalDatas.Count == 0) return true;
                var recruitIds = additionalDatas.Select(m => m.Id).ToList();
                var relatives = await _zarnicaDb.Relatives.Where(m => recruitIds.Contains(m.RecruitId) &&
                                                                      m.RelativeType == zModels.Relative.TempRelative)
                    .ToListAsync();
                var recruits = await _appDb.Recruits.Where(m => recruitIds.Contains(m.RecruitId)).ToListAsync();
                foreach (var recruit in recruits)
                {
                    var relative = relatives.FirstOrDefault(m => m.RecruitId == recruit.RecruitId);
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