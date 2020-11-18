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
    public class UserController : Controller
    {
        private readonly AppDbContext _appDb;

        public UserController(AppDbContext appDb)
        {
            _appDb = appDb;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewBag.Permissions = await _appDb.Permissions.ToListAsync();
            ViewBag.Departments = await _appDb.Departments.ToListAsync();
            ViewData["usersCount"] = await _appDb.Users.CountAsync();

            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public IActionResult IndexGrid(
            string search = "",
            int page = 1,
            int permissionId = 0,
            int departmentId = 0,
            int rows = 10,
            bool exitMode = true)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            if (exitMode) return PartialView("_IndexGrid", new List<User>());
            var query = _appDb.Users
                .Include(u => u.Department)
                .Include(u => u.UserPermissions)
                .ThenInclude(u => u.Permission)
                .AsNoTracking().AsEnumerable();
            if (permissionId != 0)
            {
                query = query.Where(u => u.UserPermissions.Any(up => up.PermissionId == permissionId));
            }

            if (departmentId != 0)
            {
                query = query.Where(u => u.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.ToLower().Contains(search.ToLower()));
            }

            var users = query.ToList();
            return PartialView("_IndexGrid", users);
        }

        public async Task<IActionResult> CreateModal()
        {
            ViewBag.Departments = await _appDb.Departments.AsNoTracking().ToListAsync();
            ViewBag.Permissions = await _appDb.Permissions.AsNoTracking().ToListAsync();
            var viewModel = new UserViewModel();
            return PartialView("_CreateModal", viewModel);
        }

        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (model.PermissionsIds != null && model.PermissionsIds.Length > 0)
            {
                var permissions =
                    await _appDb.Permissions.Where(m => model.PermissionsIds.Contains(m.Id)).ToListAsync();
                if (permissions.Count != model.PermissionsIds.Length)
                    ModelState.AddModelError("Id", "Выбраны не существующие права доступа. Обновите страницу.");
            }

            if (await _appDb.Users.AnyAsync(m => m.Login == model.Login))
                ModelState.AddModelError("Login", "Сотрудник с данным Логином уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            Debug.Assert(model.DepartmentId != null, "model.DepartmentId != null");
            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                await _appDb.Users.AddAsync(new User
                {
                    Login = model.Login,
                    Password = model.Password,
                    FullName = model.FullName,
                    DepartmentId = model.DepartmentId.Value
                });
                await _appDb.SaveChangesAsync();

                var user = await _appDb.Users.FirstOrDefaultAsync(m => m.Login == model.Login);
                if (user == null) throw new NullReferenceException();
                if (model.PermissionsIds != null)
                {
                    await _appDb.UserPermissions.AddRangeAsync(model.PermissionsIds.Select(permissionsId =>
                        new UserPermission {UserId = user.Id, PermissionId = permissionsId}).ToList());
                }

                await _appDb.SaveChangesAsync();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id", "Критическая ошибка при добавлении пользователя. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> EditModal(int id)
        {
            ViewBag.Departments = await _appDb.Departments.AsNoTracking().ToListAsync();
            ViewBag.Permissions = await _appDb.Permissions.AsNoTracking().ToListAsync();
            var user = await _appDb.Users
                .Include(m => m.UserPermissions)
                .AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) throw new NullReferenceException();
            var viewModel = new UserViewModel
            {
                Login = user.Login,
                Password = user.Password,
                FullName = user.FullName,
                DepartmentId = user.DepartmentId,
                PermissionsIds = user.UserPermissions.Select(m => m.PermissionId).ToArray()
            };
            return PartialView("_EditModal", viewModel);
        }

        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (model.PermissionsIds != null && model.PermissionsIds.Length > 0)
            {
                var permissions =
                    await _appDb.Permissions.Where(m => model.PermissionsIds.Contains(m.Id)).ToListAsync();
                if (permissions.Count != model.PermissionsIds.Length)
                    ModelState.AddModelError("Id", "Выбраны не существующие права доступа. Обновите страницу.");
            }

            var user = await _appDb.Users
                .Include(m => m.Department)
                .Include(m => m.UserPermissions)
                .ThenInclude(m => m.Permission)
                .FirstOrDefaultAsync(m => m.Id == model.Id);
            if (user == null)
                ModelState.AddModelError("Id", "Не удалось найти пользователя. Обновите страницу.");

            if (user != null && user.Login != model.Login && await _appDb.Users.AnyAsync(m => m.Login == model.Login))
                ModelState.AddModelError("Login", "Пользователь с данным Логином уже существует!");

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            Debug.Assert(model.DepartmentId != null, "model.DepartmentId != null");
            Debug.Assert(user != null, nameof(user) + " != null");


            user.Login = model.Login;
            user.Password = model.Password;
            user.FullName = model.FullName;
            user.DepartmentId = model.DepartmentId.Value;
            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                _appDb.UserPermissions.RemoveRange(user.UserPermissions);
                await _appDb.SaveChangesAsync();
                if (model.PermissionsIds != null)
                {
                    await _appDb.UserPermissions.AddRangeAsync(model.PermissionsIds.Select(permissionsId =>
                        new UserPermission {UserId = user.Id, PermissionId = permissionsId}).ToList());
                }

                _appDb.Users.Update(user);
                await _appDb.SaveChangesAsync();

                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id",
                    "Критическая ошибка при изменении данных пользователя. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _appDb.Users
                .Include(m => m.UserPermissions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Пользователь не найден!"));
                return RedirectToAction("Index");
            }

            var transaction = await _appDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                if (user.UserPermissions.Any())
                {
                    _appDb.UserPermissions.RemoveRange(user.UserPermissions);
                }

                _appDb.Users.Remove(user);
                await _appDb.SaveChangesAsync();
                transaction.Commit();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Пользователь успешно удален!"));
                return RedirectToAction("Index");
            }
            catch
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Не удалось удалить пользователя. Обратитесь к ВЦшнику!"));
                return RedirectToAction("Index");
            }
        }
    }
}