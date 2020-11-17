using System;
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

namespace NewSprt.Controllers
{
    /// <summary>
    /// Контроллер для работы с задачами
    /// </summary>
    [Authorize]
    public class WorkTaskController : Controller
    {
        private readonly AppDbContext _appDb;

        public WorkTaskController(AppDbContext appDb)
        {
            _appDb = appDb;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewData["taskCount"] = await _appDb.WorkTasks.AsNoTracking().CountAsync(m => !m.IsArchive);
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<IActionResult> IndexGrid(int page = 1, int rows = 10)
        {
            var token = User.GetToken();
            var user = await _appDb.Users.AsNoTracking().FirstOrDefaultAsync(m => m.AuthorizationToken == token);
            if (user == null) RedirectToAction("Logout", "Account");

            var tasks = await _appDb.WorkTasks.Where(t =>
                    (t.TaskManagerId == user.Id
                     || t.TaskResponsibleId == user.Id
                     || t.Department.HeadUserId == user.Id
                     || User.IsAdmin())
                    && !t.IsArchive)
                .Include(t => t.Department)
                .Include(t => t.TaskManagerUser)
                .Include(t => t.TaskResponsibleUser)
                .OrderByDescending(m => m.IsUrgent)
                .ThenByDescending(m => m.Id).AsNoTracking().ToListAsync();
            ViewBag.Pagination = new Pagination(rows, page);
            ViewBag.UserId = user?.Id;
            return PartialView("_IndexGrid", tasks);
        }

        [Authorize(Policy = "Secretary")]
        public async Task<IActionResult> Archive()
        {
            ViewData["taskCount"] = await _appDb.WorkTasks.AsNoTracking().CountAsync(m => m.IsArchive);
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }
        
        [Authorize(Policy = "Secretary")]
        public async Task<IActionResult> ArchiveGrid(int page = 1, int rows = 10)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            var tasks = await _appDb.WorkTasks
                .Include(t => t.Department)
                .Include(t => t.TaskManagerUser)
                .Include(t => t.TaskResponsibleUser)
                .OrderBy(m => m.Id)
                .AsNoTracking().Where(t => t.IsArchive).ToListAsync();
            return PartialView("_ArchiveGrid", tasks);
        }

        public async Task<IActionResult> ShowModal(int id)
        {
            var users = await _appDb.Users.Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FullName} ({u.Department.Name})"
                }).AsNoTracking().ToListAsync();
            ViewBag.Users = users;
            var workTask = await _appDb.WorkTasks.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (workTask == null) throw new NullReferenceException();
            var viewTask = new WorkTaskViewModel();
            viewTask.Set(workTask);
            return PartialView("_ShowModal", viewTask);
        }

        public async Task<IActionResult> CreateModal()
        {
            var users = await _appDb.Users
                .Where(u => u.AuthorizationToken != User.GetToken()).Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FullName} ({u.Department.ShortName})"
                }).AsNoTracking().ToListAsync();
            ViewBag.Users = users;
            return PartialView("_CreateModal", new WorkTaskViewModel());
        }

        public async Task<IActionResult> Create(WorkTaskViewModel model)
        {
            var taskManagerUser = await _appDb.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthorizationToken == User.GetToken());
            var taskResponsibleUser = await _appDb.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == model.TaskResponsibleId);

            if (taskManagerUser == null)
                RedirectToAction("Logout", "Account");
            if (taskResponsibleUser == null)
                ModelState.AddModelError("TaskResponsibleId", "Не выбран исполнитель");
            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
            
            try
            {
                if (taskManagerUser == null || taskResponsibleUser == null) throw new NullReferenceException();
                var workTask = new WorkTask()
                {
                    Name = model.DocumentName,
                    DocumentNumber = model.DocumentNumber,
                    Discription = model.Discription,
                    TaskManagerId = taskManagerUser.Id,
                    TaskResponsibleId = taskResponsibleUser.Id,
                    DepartmentId = taskResponsibleUser.DepartmentId,
                    IsRepeat = model.IsRepeat,
                    IsUrgent = model.IsUrgent,
                    CompletionDate = model.IsRepeat ? DateTime.Now.Date : model.CompletionDate.Date,
                    AdditionToDeadlines = model.IsRepeat ? model.AdditionToDeadlines : "Один раз",
                    IsArchive = false
                };
                await _appDb.WorkTasks.AddAsync(workTask);
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
            var users = await _appDb.Users
                .Where(u => u.AuthorizationToken != User.GetToken()).Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FullName} ({u.Department.ShortName})"
                }).AsNoTracking().ToListAsync();
            ViewBag.Users = users;
            var workTask = await _appDb.WorkTasks.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (workTask == null) throw new NullReferenceException();
            var viewTask = new WorkTaskViewModel();
            viewTask.Set(workTask);
            return PartialView("_EditModal", viewTask);
        }

        public async Task<IActionResult> Edit(WorkTaskViewModel model)
        {
            var user = await _appDb.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthorizationToken == User.GetToken());
            var taskResponsibleUser = await _appDb.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == model.TaskResponsibleId);


            if (taskResponsibleUser == null)
                ModelState.AddModelError("TaskResponsibleId", "Не выбран исполнитель");
            if (user == null)
                RedirectToAction("Logout", "Account");

            var task = await _appDb.WorkTasks
                .Include(t => t.Department)
                .Include(t => t.TaskManagerUser)
                .Include(t => t.TaskResponsibleUser)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (task == null)
                ModelState.AddModelError("Id", "Не удалось найти задачу по Id. Обновите страницу.");
            else if (! (User.IsAdmin()
                       || User.IsPermission(Permission.Secretary)
                       || user?.Id == task.TaskManagerId
                       || task.Department.HeadUserId == user?.Id))
                ModelState.AddModelError("Id", "Недостаточно прав для изменения задачи");

            if (!ModelState.IsValid) return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            
            try
            {
                if (task == null) throw new NullReferenceException();
                task.Set(model, user, taskResponsibleUser);
                _appDb.WorkTasks.Update(task);
                await _appDb.SaveChangesAsync();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        public async Task<IActionResult> DeleteMoveToArchive(int id)
        {
            var task = await _appDb.WorkTasks.FirstOrDefaultAsync(w => w.Id == id);
            if (task == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Задача не найдена!"));
                return RedirectToAction("Index");
            }
            
            var user = await _appDb.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthorizationToken == User.GetToken());

            if (!(User.IsAdmin()
                  || User.IsPermission(Permission.Secretary)
                  || user?.Id == task.TaskManagerId
                  || task.Department.HeadUserId == user?.Id))
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Недостаточно прав для удаления задачи в архив!"));
                return RedirectToAction("Index");
            }

            task.IsArchive = true;
            task.IsUrgent = false;
            _appDb.WorkTasks.Update(task);
            await _appDb.SaveChangesAsync();
            HttpContext.Session.Set("alert",
                new AlertViewModel(AlertType.Success, "Задача успешно убрана в архив!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ReturnFromArchive(int id)
        {
            var task = await _appDb.WorkTasks.FirstOrDefaultAsync(w => w.Id == id);
            if (task == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Задача не найдена!"));
                return RedirectToAction("Archive");
            }

            if (!(User.IsAdmin()
                  || User.IsPermission(Permission.Secretary)))
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Недостаточно прав для востановления задачи из архива!"));
                return RedirectToAction("Archive");
            }

            task.IsArchive = false;
            _appDb.WorkTasks.Update(task);
            await _appDb.SaveChangesAsync();
            HttpContext.Session.Set("alert",
                new AlertViewModel(AlertType.Success, "Задача успешно востановлена из архива!"));
            return RedirectToAction("Archive");
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _appDb.WorkTasks.FirstOrDefaultAsync(w => w.Id == id);
            if (task == null)
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Задача не найдена!"));
                return RedirectToAction("Archive");
            }
            
            if (!(User.IsAdmin()
                  || User.IsPermission(Permission.Secretary)))
            {
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error,
                        "Недостаточно прав для удаления задачи из архива!"));
                return RedirectToAction("Archive");
            }
            
            _appDb.WorkTasks.Remove(task);
            await _appDb.SaveChangesAsync();
            HttpContext.Session.Set("alert",
                new AlertViewModel(AlertType.Success, "Задача успешно удалена!"));
            return RedirectToAction("Archive");
        }
    }
}