using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using NewSprt.Models.Extensions;
using NewSprt.Models.Managers;
using NewSprt.ViewModels;

namespace NewSprt.Controllers
{
    [Authorize]
    public class RecruitController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;
        private readonly RecruitManager _recruitManager;

        public RecruitController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
            _recruitManager = new RecruitManager(appDb, zarnicaDb);
        }

        [Authorize(Policy = Permission.Admin)]
        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            var conscriptionPeriods = await _appDb.ConscriptionPeriods.AsNoTracking().ToListAsync();
            ViewBag.ConscriptionPeriods = conscriptionPeriods;
            ViewData["recruitsCount"] = await _appDb.Recruits.CountAsync(m =>
                m.ConscriptionPeriodId == conscriptionPeriods.FirstOrDefault(c => !c.IsArchive).Id);
            await _recruitManager.SynchronizationOfDatabases();
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }
        
        [Authorize(Policy = Permission.Admin)]
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
                .ThenBy(m => m.MilitaryComissariatCode).ThenBy(m => m.LastName)
                .AsNoTracking().ToListAsync();

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

        public async Task<IActionResult> Show(int id)
        {
            var appRecruit = await _appDb.Recruits
                .Include(m => m.ConscriptionPeriod)
                .Include(m => m.DactyloscopyStatus)
                .AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            var currentConscriptionPeriod =
                await _appDb.ConscriptionPeriods.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == appRecruit.ConscriptionPeriodId);
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
                .AsNoTracking().FirstOrDefaultAsync(m =>
                    m.Id == appRecruit.RecruitId && m.Code == appRecruit.UniqueRecruitNumber);
            appRecruit.ZRecruit = recruit;
            return View(appRecruit);
        }

        public IActionResult ShowArchive(int id)
        {
            return View();
        }
        
        public async Task<IActionResult> GetRecruitsBySearch(int conscriptionPeriodId, string q)
        {
            var recruits = await _appDb.Recruits
                .Where(m => m.ConscriptionPeriodId == conscriptionPeriodId)
                .OrderBy(m => m.LastName).AsNoTracking().ToListAsync();

            if (string.IsNullOrEmpty(q))
                return new JsonResult(recruits.Select(m => new
                {
                    id = m.Id,
                    text = m.FullName,
                }).ToList());
            
            var searchArr = q.Split(" ");
            switch (searchArr.Length)
            {
                case 1:
                    recruits = recruits.Where(m => m.LastName.ToLower().StartsWith(q.ToLower())).ToList();
                    break;
                case 2:
                    recruits = recruits.Where(m =>
                        m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                        m.FirstName.ToLower().StartsWith(searchArr[1].ToLower())).ToList();
                    break;
                case 3:
                    recruits = recruits.Where(m =>
                        m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                        m.FirstName.ToLower().StartsWith(searchArr[1].ToLower()) &&
                        m.Patronymic.ToLower().StartsWith(searchArr[2].ToLower())).ToList();
                    break;
            }

            return new JsonResult(recruits.Select(m => new
            {
                id = m.Id,
                text = m.FullName,
            }).ToList());
        }

        public async Task<IActionResult> GetRecruitStatus(int recruitId)
        {
            var appRecruit = await _appDb.Recruits.FirstOrDefaultAsync(m => m.Id == recruitId);
            var zRecruit = await _zarnicaDb.Recruits
                .Include(m => m.Team)
                .ThenInclude(m => m.MilitaryUnit)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.Id == appRecruit.RecruitId);
            return new JsonResult(zRecruit.Status);
        }
    }
}