using Microsoft.AspNetCore.Mvc;

namespace NewSprt.Controllers
{
    public class DactyloscopyController : Controller
    {
        public  IActionResult EditDactyloscopyStatus(int recruitId, int editStatus)
        {
            return RedirectToAction("Show", "Recruit", new {id = recruitId});
        }

        public IActionResult PrintDactyloscopyCard(int recruitId)
        {
            return RedirectToAction("Show", "Recruit", new {id = recruitId});
        }
    }
}