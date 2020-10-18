using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.ViewModels;

namespace NewSprt.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext _appDb;

        public AccountController(AppDbContext appDbContext)
        {
            _appDb = appDbContext;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _appDb.Users
                    .Include(m => m.UserPermissions)
                    .ThenInclude(m => m.Permission)
                    .FirstOrDefaultAsync(u =>
                    u.Login == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Login, string.Join(",",user.UserPermissions.Select(m => m.Permission.ShortName)));

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некоректные логин и(или) пароль");
            }

            return View(model);
        }

        private async Task Authenticate(string userName, string permissions = "")
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim("Permissions", permissions)
            };
            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}