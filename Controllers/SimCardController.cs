using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using NewSprt.Models.Helper.Documents;
using NewSprt.Models.Managers;
using NewSprt.ViewModels;
using zModels = NewSprt.Data.Zarnica.Models;

namespace NewSprt.Controllers
{
    [Authorize(Policy = Permission.SimCard)]
    public class SimCardController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;
        private readonly RecruitManager _recruitManager;

        public SimCardController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
            _recruitManager = new RecruitManager(appDb, zarnicaDb);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            var conscriptionPeriodId =
                (await _appDb.ConscriptionPeriods.AsNoTracking().FirstOrDefaultAsync(c => !c.IsArchive)).Id;
            ViewBag.ConscriptionPeriodId = conscriptionPeriodId;
            ViewData["recruitsCount"] = await _appDb.Recruits.AsNoTracking().CountAsync(m =>
                m.ConscriptionPeriodId == conscriptionPeriodId);
            await _recruitManager.SynchronizationOfDatabases();
            return View();
        }

        public async Task<IActionResult> IndexGrid(
            string militaryComissariatId = "",
            int conscriptionPeriodId = 0,
            int page = 1,
            int rows = 10,
            bool isNotPhone = false,
            string search = "",
            bool exitMode = true)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            if (exitMode) return PartialView("_IndexGrid", new List<Recruit>());
            var query = _appDb.Recruits.Include(m => m.MilitaryComissariat).AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                query = query.Where(m => m.MilitaryComissariatCode == militaryComissariatId);
            }

            if (conscriptionPeriodId != 0)
            {
                query = query.Where(m => m.ConscriptionPeriodId == conscriptionPeriodId);
            }

            var appRecruits = await query.AsNoTracking()
                .OrderByDescending(m => m.DeliveryDate)
                .ThenBy(m => m.MilitaryComissariatCode).ThenBy(m => m.LastName).ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                var searchArr = search.Split(" ");
                switch (searchArr.Length)
                {
                    case 1:
                        appRecruits = appRecruits.Where(m => m.LastName.ToLower().StartsWith(search.ToLower()))
                            .ToList();
                        break;
                    case 2:
                        appRecruits = appRecruits.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower())).ToList();
                        break;
                    case 3:
                        appRecruits = appRecruits.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower()) &&
                            m.Patronymic.ToLower().StartsWith(searchArr[2].ToLower())).ToList();
                        break;
                }
            }

            var recruitsIds = appRecruits.Select(r => r.RecruitId);
            var zRecruits = await _zarnicaDb.Recruits
                .Where(m => recruitsIds.Contains(m.Id))
                .AsNoTracking().ToListAsync();
            foreach (var recruit in zRecruits)
            {
                if (string.IsNullOrEmpty(recruit.HomePhone) ||
                    string.IsNullOrEmpty(recruit.MobilePhone))
                {
                    recruit.IsNotPhone = true;
                }

                appRecruits.First(m => m.RecruitId == recruit.Id).ZRecruit = recruit;
            }

            if (isNotPhone) appRecruits = appRecruits.Where(m => m.ZRecruit.IsNotPhone).ToList();

            return PartialView("_IndexGrid", appRecruits);
        }
        
        public async Task<IActionResult> RecruitsPhoneReportModal()
        {
            var militaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            return PartialView("_RecruitsPhoneListModal", militaryComissariats);
        }

        public async Task<IActionResult> PrintRecruitsPhoneReport(
            DateTime startDate, 
            DateTime endDate,
            string printMode = "today", 
            string militaryComissariatId = "")
        {
            string header;
            var qRecruits = _zarnicaDb.Recruits.Include(m => m.MilitaryComissariat).AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                qRecruits = qRecruits.Where(m => m.MilitaryComissariatId == militaryComissariatId);
            }
            switch (printMode)
            {
                case "today":
                    qRecruits = qRecruits.Where(m => m.DelivaryDate.DayOfYear == DateTime.Today.DayOfYear);
                    header = DateTime.Today.ToShortDateString();
                    break;
                case "full":
                    var conscriptionPeriod = await _appDb.ConscriptionPeriods.AsNoTracking().FirstOrDefaultAsync(c => !c.IsArchive);
                    header = conscriptionPeriod.Name;
                    break;
                case "period":
                    qRecruits = qRecruits.Where(m => m.DelivaryDate.DayOfYear > startDate.DayOfYear);
                    header = "период с " + startDate.ToShortDateString() + " по " + endDate.ToShortDateString();
                    break;
                default:
                    return RedirectToAction("Index");
            }
            
            var recruits = await qRecruits.ToListAsync();
            if (!string.IsNullOrEmpty(militaryComissariatId))
                header += " из " + recruits[0].MilitaryComissariat.ShortName;
            return File(ExcelDocumentHelper.GenerateRecruitsPhoneReport(recruits, header),
                ExcelDocumentHelper.OutputFormatType,
                $"Список призывников.xlsx");
        }
    }
}