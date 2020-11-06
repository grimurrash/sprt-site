using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewSprt.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    [Authorize("Admin")]
    public class DepartmentController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}