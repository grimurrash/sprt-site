using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _appDb;

        public DepartmentController(AppDbContext appDb)
        {
            _appDb = appDb;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewData["departmentsCount"] = await _appDb.Departments.CountAsync();

            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<IActionResult> IndexGrid(
            int page = 1,
            int rows = 10,
            bool exitMode = true)
        {
            if (exitMode) return PartialView("_IndexGrid", new List<Department>());
            ViewBag.Pagination = new Pagination(rows, page);
            var departments = await _appDb.Departments
                .Include(m => m.HeadUser).ToListAsync();
            return PartialView("_IndexGrid", departments);
        }

        public async Task<IActionResult> CreateModal()
        {
            ViewBag.Users = await _appDb.Users.Where(m => m.Department.HeadUserId != m.Id).ToListAsync();
            return PartialView("_CreateModal", new DepartmentViewModel());
        }

        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (await _appDb.Departments.AnyAsync(m => m.ShortName == model.ShortName || m.Name == model.Name))
                ModelState.AddModelError("Name", "Отделение с данным наименованием уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                Debug.Assert(model.HeadUserId != null, "model.HeadUserId != null");
                var department = new Department
                {
                    ShortName = model.ShortName,
                    Name = model.Name,
                    HeadUserId = model.HeadUserId.Value
                };
                await _appDb.Departments.AddAsync(department);
                await _appDb.SaveChangesAsync();
                var user = await _appDb.Users.FirstOrDefaultAsync(m => m.Id == model.HeadUserId.Value);
                user.DepartmentId = department.Id;
                _appDb.Users.Update(user);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id", "Критическая ошибка при добавлении отделения. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> EditModal(int id)
        {
            var department = await _appDb.Departments.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null) throw new NullReferenceException();
            ViewBag.Users = await _appDb.Users.Where(m => m.DepartmentId == id).ToListAsync();
            return PartialView("_EditModal",
                new DepartmentViewModel
                {
                    ShortName = department.ShortName, Name = department.Name, HeadUserId = department.HeadUserId,
                    Id = department.Id
                });
        }

        public async Task<IActionResult> Edit(DepartmentViewModel model)
        {
            var department = await _appDb.Departments.FirstOrDefaultAsync(m => m.Id == model.Id);
            if (department == null)
                ModelState.AddModelError("Id", "Отделение не найдено. Обновите страницу!");
            if (department != null && await _appDb.Departments.AnyAsync(m =>
                m.Id != model.Id && (m.ShortName == model.ShortName || m.Name == model.Name)))
                ModelState.AddModelError("Name", "Отделение с данным наименованием уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                if (department == null) throw new NullReferenceException();
                department.Name = model.Name;
                department.ShortName = model.ShortName;
                Debug.Assert(model.HeadUserId != null, "model.HeadUserId != null");
                department.HeadUserId = model.HeadUserId.Value;
                _appDb.Departments.Update(department);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id", "Критическая ошибка при добавлении отделения. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _appDb.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Отделение не найдено!"));
                return RedirectToAction("Index");
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var users = await _appDb.Users.Where(m => m.DepartmentId == department.Id).ToListAsync();
                foreach (var user in users)
                {
                    user.DepartmentId = 1;
                }

                _appDb.Users.UpdateRange(users);
                _appDb.Departments.Remove(department);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Отеделение успешно удалено!"));
                return RedirectToAction("Index");
            }
            catch
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Не удалось удалить отделение. Обратитесь к ВЦшнику!"));
                return RedirectToAction("Index");
            }
        }
    }
}