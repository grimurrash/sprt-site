using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.ViewModels.FormModels;

namespace NewSprt.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Context для работы с базой приложения (сайта)
        /// </summary>
        private readonly AppDbContext _appDb;

        public AccountController(AppDbContext appDbContext)
        {
            _appDb = appDbContext;
        }
        
        /// <summary>
        /// Страница авторизации
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        /// <summary>
        /// Авторизация пользователя по Login и Password
        /// </summary>
        /// <param name="model">ViewModel для проверки валидации</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = await _appDb.Users
                .Include(m => m.UserPermissions)
                .ThenInclude(m => m.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Login == model.Login && u.Password == model.Password);
            if (user != null)
            {
                await Authenticate(model.Login, string.Join(",",user.UserPermissions.Select(m => m.Permission.ShortName)));
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Некоректные логин и(или) пароль");
            return View(model);
        }

        /// <summary>
        /// Авторизация пользователя, сохранения информации о пользователе в Cookie
        /// </summary>
        /// <param name="userName">Логин пользователя</param>
        /// <param name="permissions">Пароль пользователя</param>
        /// <returns></returns>
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

        /// <summary>
        /// Выход пользователя из системы, удаление данных о пользователе из Cookie
        /// </summary>
        /// <returns>Переход на страницу авторизации</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}