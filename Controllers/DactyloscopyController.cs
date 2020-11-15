using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using NewSprt.Models.Extensions;
using NewSprt.Models.Helper.Documents;
using NewSprt.Models.Managers;
using NewSprt.ViewModels;

namespace NewSprt.Controllers
{
    [Authorize(Policy = Permission.Dactyloscopy)]
    public class DactyloscopyController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;
        private readonly RecruitManager _recruitManager;

        public DactyloscopyController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
            _recruitManager = new RecruitManager(appDb, zarnicaDb);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            var conscriptionPeriods = await _appDb.ConscriptionPeriods.AsNoTracking().ToListAsync();
            ViewBag.ConscriptionPeriods = conscriptionPeriods;
            ViewBag.DactyloscopyStatuses = await _appDb.DactyloscopyStatuses.AsNoTracking().ToListAsync();
            ViewData["recruitsCount"] = await _appDb.Recruits.AsNoTracking().CountAsync(m =>
                m.ConscriptionPeriodId == conscriptionPeriods.FirstOrDefault(c => !c.IsArchive).Id);
            await _recruitManager.SynchronizationOfDatabases();
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<IActionResult> IndexGrid(
            string militaryComissariatId = "",
            int conscriptionPeriodId = 0,
            int page = 1,
            int rows = 10,
            string search = "")
        {
            ViewBag.Pagination = new Pagination(rows, page);
            var query = _appDb.Recruits
                .Include(m => m.DactyloscopyStatus)
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.ConscriptionPeriod).AsQueryable();
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

            if (string.IsNullOrEmpty(search)) return PartialView("_IndexGrid", appRecruits);

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

            return PartialView("_IndexGrid", appRecruits);
        }

        public async Task<IActionResult> EditDactyloscopyStatus(int recruitId, int editStatus)
        {
            var recruit = await _appDb.Recruits.FirstOrDefaultAsync(m => m.Id == recruitId);
            if (editStatus == recruit.DactyloscopyStatusId)
                return RedirectToAction("Show", "Recruit", new {id = recruitId});
            recruit.DactyloscopyStatusId = editStatus;
            _appDb.Recruits.Update(recruit);
            await _appDb.SaveChangesAsync();
            return RedirectToAction("Show", "Recruit", new {id = recruitId});
        }

        public async Task<IActionResult> PrintDactyloscopyCard(int recruitId)
        {
            var recruit = await _appDb.Recruits.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == recruitId);
            var zRecruit = await _zarnicaDb.Recruits
                .Include(m => m.Settlement)
                .Include(m => m.AdditionalData)
                .AsNoTracking().FirstOrDefaultAsync(m =>
                    m.Code == recruit.UniqueRecruitNumber && m.Id == recruit.RecruitId &&
                    m.DelivaryDate.DayOfYear == recruit.DeliveryDate.DayOfYear
                );
            if (zRecruit != null)
                return File(WordDocumentHelper.GenerateDactyloscopyRecruitCard(zRecruit),
                    WordDocumentHelper.OutputFormatType,
                    $"Дактокарта {zRecruit.LastName} {zRecruit.FirstName[0]}.{zRecruit.Patronymic[0]}.docx");

            HttpContext.Session.Set("alert",
                new AlertViewModel(AlertType.Error,
                    "Призывник в Зарнице не найден!"));
            return RedirectToAction("Show", "Recruit", new {id = recruitId});
        }

        public async Task<IActionResult> PrintMilitaryComissariatReport(string militaryComissariatId,
            int conscriptionPeriodId)
        {
            var militaryComissariat = await _appDb.MilitaryComissariats.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == militaryComissariatId);
            var recruits = await _appDb.Recruits
                .Where(m => m.MilitaryComissariatCode == militaryComissariatId &&
                            m.ConscriptionPeriodId == conscriptionPeriodId)
                .OrderBy(m => m.LastName).AsNoTracking().ToListAsync();
            return File(ExcelDocumentHelper.GenerateMilitaryComissariatReport(recruits, militaryComissariat),
                ExcelDocumentHelper.OutputFormatType,
                "Именной список.xlsx");
        }

        public async Task<IActionResult> ConscriptionPeriodReport()
        {
            var conscriptionPeriods = await _appDb.ConscriptionPeriods.AsNoTracking().ToListAsync();
            ViewBag.ConscriptionPeriods = conscriptionPeriods;
            ViewBag.SelectedConscriptionPeriodId = conscriptionPeriods.FirstOrDefault(m => !m.IsArchive)?.Id;
            return PartialView("_ConscriptionPeriodReportModal");
        }

        public async Task<IActionResult> PrintConscriptionPeriodReport(int conscriptionPeriodId,
            string dateAndOutgoingNumber)
        {
            var recruits = await _appDb.Recruits
                .Where(m => m.ConscriptionPeriodId == conscriptionPeriodId &&
                            m.DactyloscopyStatusId == DactyloscopyStatus.Selected)
                .Include(m => m.MilitaryComissariat)
                .AsNoTracking().ToListAsync();
            return File(ExcelDocumentHelper.GenerateConscriptionPeriodReport(recruits, dateAndOutgoingNumber),
                ExcelDocumentHelper.OutputFormatType,
                "Журнал учета военослужащих.xlsx");
        }
    }
}