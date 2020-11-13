using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Models.Extensions;
using NewSprt.ViewModels;
using NewSprt.ViewModels.FormModels;

namespace NewSprt.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize("Admin")]
    public class СonscriptionPeriodController : Controller
    {
        private readonly AppDbContext _appDb;

        public СonscriptionPeriodController(AppDbContext appDb)
        {
            _appDb = appDb;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["conscriptionPeriodCount"] = await _appDb.ConscriptionPeriods.CountAsync();

            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<IActionResult> IndexGrid(
            int page = 1,
            int rows = 10)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            var conscriptionPeriods = await _appDb.ConscriptionPeriods
                .OrderBy(m => m.IsArchive)
                .ThenByDescending(m => m.Id).ToListAsync();
            return PartialView("_IndexGrid", conscriptionPeriods);
        }

        public IActionResult CreateModal()
        {
            return PartialView("_CreateModal", new ConscriptionPeriodViewModel());
        }

        public async Task<IActionResult> Create(ConscriptionPeriodViewModel model)
        {
            if (await _appDb.Departments.AnyAsync(m => m.Name == model.Name))
                ModelState.AddModelError("Name", "Период призыва с данным наименованием уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var conscriptionPeriod = new ConscriptionPeriod
                {
                    Name = model.Name,
                    IsArchive = model.IsArchive
                };
                if (!model.IsArchive)
                {
                    var allConscriptionPeriods = await _appDb.ConscriptionPeriods.ToListAsync();
                    foreach (var cPeriod in allConscriptionPeriods)
                    {
                        cPeriod.IsArchive = true;
                    }
                    _appDb.ConscriptionPeriods.UpdateRange(allConscriptionPeriods);
                }
                await _appDb.ConscriptionPeriods.AddAsync(conscriptionPeriod);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id",
                    "Критическая ошибка при добавлении периода призыва. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> EditModal(int id)
        {
            var conscriptionPeriod = await _appDb.ConscriptionPeriods.FirstOrDefaultAsync(m => m.Id == id);
            if (conscriptionPeriod == null) throw new NullReferenceException();
            return PartialView("_EditModal",
                new ConscriptionPeriodViewModel
                {
                    Name = conscriptionPeriod.Name, IsArchive = conscriptionPeriod.IsArchive, Id = conscriptionPeriod.Id
                });
        }

        public async Task<IActionResult> Edit(ConscriptionPeriodViewModel model)
        {
            var conscriptionPeriod = await _appDb.ConscriptionPeriods.FirstOrDefaultAsync(m => m.Id == model.Id);
            if (conscriptionPeriod == null)
                ModelState.AddModelError("Id", "Период призыва не найден. Обновите страницу!");
            if (conscriptionPeriod != null && await _appDb.ConscriptionPeriods.AnyAsync(m =>
                m.Id != model.Id && m.Name == model.Name))
                ModelState.AddModelError("Name", "Период призыва с данным наименованием уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                if (conscriptionPeriod == null) throw new NullReferenceException();
                conscriptionPeriod.Name = model.Name;
                conscriptionPeriod.IsArchive = model.IsArchive;
                if (!model.IsArchive)
                {
                    var allConscriptionPeriods = await _appDb.ConscriptionPeriods.ToListAsync();
                    foreach (var cPeriod in allConscriptionPeriods)
                    {
                        cPeriod.IsArchive = true;
                    }
                    _appDb.ConscriptionPeriods.UpdateRange(allConscriptionPeriods);
                }
                _appDb.ConscriptionPeriods.Update(conscriptionPeriod);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id",
                    "Критическая ошибка при добавлении периода призыва. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var conscriptionPeriod = await _appDb.ConscriptionPeriods
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conscriptionPeriod == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Период призыва не найден!"));
                return RedirectToAction("Index");
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                if (conscriptionPeriod.RecruitsCount > 0)
                {
                    HttpContext.Session.Set("alert",
                        new AlertViewModel(AlertType.Error,
                            "В архиве есть призывники!"));
                    return RedirectToAction("Index");
                }

                _appDb.ConscriptionPeriods.Remove(conscriptionPeriod);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Период призыва успешно удален!"));
                return RedirectToAction("Index");
            }
            catch
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Не удалось удалить период призыва. Обратитесь к ВЦшнику!"));
                return RedirectToAction("Index");
            }
        }
    }
}