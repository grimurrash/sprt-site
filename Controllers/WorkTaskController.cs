using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewSprt.Data.App;
using NewSprt.ViewModels.FormModels;

namespace NewSprt.Controllers
{
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
            return View();
        }

        public async Task<IActionResult> Archive()
        {
            return View();
        }

        public async Task<IActionResult> CreateModal()
        {
            return PartialView("_CreateModal");
        }

        public async Task<IActionResult> Create(WorkTaskViewModel model)
        {
            return new JsonResult("1");
        }

        public async Task<IActionResult> EditModal()
        {
            return PartialView("_EditModal");
        }

        public async Task<IActionResult> Edit(WorkTaskViewModel model)
        {
            return new JsonResult("1");
        }

        public async Task<IActionResult> EditWorkTaskStatus(int workTaskId, int statusId)
        {
            return new JsonResult("");
        }

        public async Task<IActionResult> EditWorkTaskStatus(int workTaskId)
        {
            return new JsonResult("");
        }
    }
}