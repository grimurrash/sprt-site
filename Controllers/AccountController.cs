using Microsoft.AspNetCore.Mvc;

namespace NewSprt.Controllers
{
    public class AccountController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}