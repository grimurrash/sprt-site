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
using NewSprt.Models.Extensions;
using NewSprt.Models.Helper.Documents;
using zModels = NewSprt.Data.Zarnica.Models;
using NewSprt.ViewModels;
using NewSprt.ViewModels.FormModels;

namespace NewSprt.Controllers
{
    [Authorize(Policy = Permission.Dismissals)]
    public class DismissalController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;

        public DismissalController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            ViewData["dismissalsCount"] = await _appDb.Dismissals.CountAsync();
            return View();
        }

        public async Task<IActionResult> IndexGrid(string militaryComissariatId = "",
            string search = "",
            bool isReturnToday = false,
            bool isReturn = false,
            bool isSend = false,
            int page = 1,
            int rows = 10,
            bool exitMode = false)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            if (exitMode) return PartialView("_IndexGrid", new List<Dismissal>());

            var query = _appDb.Dismissals
                .Include(m => m.Recruit)
                .ThenInclude(m => m.MilitaryComissariat)
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                query = query.Where(m => m.Recruit.MilitaryComissariatCode == militaryComissariatId);
            }

            if (isReturnToday)
            {
                query = query.Where(m => m.ReturnDate.DayOfYear == DateTime.Now.DayOfYear);
            }
            var dismissals = await query.OrderBy(m => m.Recruit.LastName).ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                var searchArr = search.Split(" ");
                switch (searchArr.Length)
                {
                    case 1:
                        dismissals = dismissals.Where(m => m.Recruit.LastName.ToLower().StartsWith(search.ToLower()))
                            .ToList();
                        break;
                    case 2:
                        dismissals = dismissals.Where(m =>
                            m.Recruit.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.Recruit.FirstName.ToLower().StartsWith(searchArr[1].ToLower())).ToList();
                        break;
                    case 3:
                        dismissals = dismissals.Where(m =>
                            m.Recruit.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.Recruit.FirstName.ToLower().StartsWith(searchArr[1].ToLower()) &&
                            m.Recruit.Patronymic.ToLower().StartsWith(searchArr[2].ToLower())).ToList();
                        break;
                }
            }

            var dismissalsIds = dismissals.Select(d => d.Recruit.RecruitId);
            var zRecruits = await _zarnicaDb.Recruits
                .Include(m => m.Team)
                .ThenInclude(m => m.MilitaryUnit)
                .Include(m => m.Events)
                .Where(m => dismissalsIds.Contains(m.Id))
                .ToListAsync();
            foreach (var zRecruit in zRecruits)
            {
                var dismissal = dismissals.First(m => m.RecruitId == zRecruit.Id);
                dismissal.Recruit.ZRecruit = zRecruit;
                if ((zRecruit.LastEvent.EventCode == 113 || zRecruit.LastEvent.EventCode == 112) 
                    && zRecruit.Team?.SendDate != null && zRecruit.Team.SendDate.Value.DayOfYear < DateTime.Now.DayOfYear)
                {
                    dismissal.IsSend = true;
                }
                else if (dismissal.LastEventCode != zRecruit.LastEvent.EventCode 
                         && dismissal.LastEventDate.DayOfYear != zRecruit.LastEvent.Date.DayOfYear)
                {
                    dismissal.IsReturn = true;
                }
            }

            if (isReturn) dismissals = dismissals.Where(m => m.IsReturn).ToList();
            if (isSend) dismissals = dismissals.Where(m => m.IsSend).ToList();
            
            return PartialView("_IndexGrid", dismissals);
        }

        public async Task<IActionResult> CreateModal()
        {
            ViewBag.ConscriptionPeriodId =
                (await _appDb.ConscriptionPeriods.AsNoTracking().FirstOrDefaultAsync(m => !m.IsArchive)).Id;
            return PartialView("_CreateModal", new DismissalViewModel
            {
                SendDismissalDate = DateTime.Now
            });
        }

        

        public async Task<IActionResult> Create(DismissalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
            try
            {
                var appRecruit = await _appDb.Recruits.FirstOrDefaultAsync(m => m.Id == model.RecruitId);
                var zRecruit = await _zarnicaDb.Recruits
                    .Include(m => m.Events)
                    .FirstOrDefaultAsync(m => m.Id == appRecruit.RecruitId);
                
                var dismissal = new Dismissal
                {
                    RecruitId = model.RecruitId,
                    SendDismissalDate = model.SendDismissalDate,
                    ReturnDate = model.ReturnDate,
                    Notice = model.Notice,
                    LastEventCode = zRecruit.LastEvent.EventCode,
                    LastEventDate = zRecruit.LastEvent.Date
                };
                await _appDb.Dismissals.AddAsync(dismissal);
                await _appDb.SaveChangesAsync();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> EditModal(int id)
        {
            ViewBag.ConscriptionPeriodId =
                (await _appDb.ConscriptionPeriods.AsNoTracking().FirstOrDefaultAsync(m => !m.IsArchive)).Id;
            var dismissal = await _appDb.Dismissals.FirstOrDefaultAsync(m => m.Id == id);
            ViewBag.Recruits = await _appDb.Recruits.Where(m => m.Id == dismissal.RecruitId).Select(m => new {m.Id, m.FullName}).ToListAsync();
            return PartialView("_EditModal", new DismissalViewModel
            {
                Id = dismissal.Id,
                RecruitId = dismissal.RecruitId,
                SendDismissalDate = dismissal.SendDismissalDate,
                ReturnDate = dismissal.ReturnDate,
                Notice = dismissal.Notice,
            });
        }

        public async Task<IActionResult> Edit(DismissalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
            try
            {
                var dismissal = await _appDb.Dismissals.FirstOrDefaultAsync(m => m.Id == model.Id);
                var appRecruit = await _appDb.Recruits.FirstOrDefaultAsync(m => m.Id == model.RecruitId);
                var zRecruit = await _zarnicaDb.Recruits
                    .Include(m => m.Events)
                    .FirstOrDefaultAsync(m => m.Id == appRecruit.RecruitId);
                dismissal.RecruitId = model.RecruitId;
                dismissal.SendDismissalDate = model.SendDismissalDate;
                dismissal.ReturnDate = model.ReturnDate;
                dismissal.Notice = model.Notice;
                dismissal.LastEventCode = zRecruit.LastEvent.EventCode;
                dismissal.LastEventDate = zRecruit.LastEvent.Date;
                _appDb.Dismissals.Update(dismissal);
                await _appDb.SaveChangesAsync();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dismissal = await _appDb.Dismissals.FirstOrDefaultAsync(m => m.Id == id);
                if (dismissal == null)
                {
                    HttpContext.Session.Set("alert",
                        new AlertViewModel(AlertType.Error,
                            "Увольнительная не найдена!"));
                    return RedirectToAction("Index");
                }

                _appDb.Dismissals.Remove(dismissal);
                await _appDb.SaveChangesAsync();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Увольнительная успешно удалена!"));
                return RedirectToAction("Index");
            }
            catch
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error, "Ошибка при удалении требования!"));
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> PrintDismissalRecruitsList()
        {
            var dismissals = await _appDb.Dismissals
                .Include(m => m.Recruit)
                .ThenInclude(m => m.MilitaryComissariat)
                .ToListAsync();
            return File(ExcelDocumentHelper.GenerateDismissalRecruitsList(dismissals),
                ExcelDocumentHelper.OutputFormatType,
                "Списка призывников в увольнении.xlsx");
        }

        public async Task<IActionResult> PrintReturnTodayDismissalRrcruitsList()
        {
            var dismissals = await _appDb.Dismissals
                .Include(m => m.Recruit)
                .ThenInclude(m => m.MilitaryComissariat)
                .Where(m => m.ReturnDate.DayOfYear == DateTime.Now.DayOfYear)
                .ToListAsync();
            return File(ExcelDocumentHelper.GenerateReturnTodayDismissalRrcruitsList(dismissals),
                ExcelDocumentHelper.OutputFormatType,
                "Списка призывников, возвращающихся с увольнения.xlsx");
        }
    }
}