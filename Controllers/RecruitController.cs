using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.Zarnica;
using NewSprt.Models.Managers;
using NewSprt.ViewModels;

namespace NewSprt.Controllers
{
    public class RecruitController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;
        private readonly RecruitManager _recruitManager;

        public RecruitController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
            _recruitManager = new RecruitManager(zarnicaDb, appDb);
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.ToListAsync();
            var conscriptionPeriods = await _appDb.ConscriptionPeriods.ToListAsync();
            ViewBag.ConscriptionPeriods = conscriptionPeriods;
            ViewData["recruitsCount"] = await _appDb.Recruits.CountAsync(m =>
                m.ConscriptionPeriodId == conscriptionPeriods.FirstOrDefault(c => !c.IsArchive).Id);
            await _recruitManager.SynchronizationOfDatabases();
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

            var appRecruits = await query.OrderByDescending(m => m.DeliveryDate)
                .ThenBy(m => m.MilitaryComissariatCode).ThenBy(m => m.LastName).ToListAsync();

            if (string.IsNullOrEmpty(search)) return PartialView("Grid/_IndexGrid", appRecruits);

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

            return PartialView("Grid/_IndexGrid", appRecruits);
        }

        public async Task<IActionResult> Show(int id)
        {
            var appRecruit = await _appDb.Recruits.Include(m => m.DactyloscopyStatus)
                .FirstOrDefaultAsync(m => m.Id == id);
            var currentConscriptionPeriod =
                await _appDb.ConscriptionPeriods.FirstOrDefaultAsync(m => m.Id == appRecruit.ConscriptionPeriodId);
            if (currentConscriptionPeriod.IsArchive) return RedirectToAction("ShowArchive", id);
            var recruit = await _zarnicaDb.Recruits
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.AdditionalData)
                .Include(m => m.Events)
                .Include(m => m.Settlement)
                .Include(m => m.Team)
                .ThenInclude(m => m.ArmyType)
                .Include(m => m.Team)
                .ThenInclude(m => m.MilitaryUnit)
                .FirstOrDefaultAsync(m =>
                    m.Id == appRecruit.RecruitId && m.Code == appRecruit.UniqueRecruitNumber);
            appRecruit.ZRecruit = recruit;
            return View(appRecruit);
        }

        public IActionResult ShowArchive(int id)
        {
            return View();
        }
    }
}