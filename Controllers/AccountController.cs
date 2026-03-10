using Microsoft.AspNetCore.Mvc;

namespace UniversityUserApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string login, string password)
        {
            // Никакой проверки нет — это прототип
            // Просто перенаправляем на главную страницу
            return RedirectToAction("Index", "Home");
        }
    }
}
