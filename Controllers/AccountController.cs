using BookMyRoom.Models;
using BookMyRoom.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookMyRoom.Controllers
{
    public class AccountController : Controller
    {
        private const string SessionLoginKey = "LOGIN";
        public static string? GetCurrentLogin(HttpContext httpContext) => httpContext.Session.GetString(SessionLoginKey);
        public static bool IsAdmin(HttpContext httpContext) => string.Equals(GetCurrentLogin(httpContext), "admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet("Account/Login/{login}")]
        public IActionResult Login(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return BadRequest();
            HttpContext.Session.SetString(SessionLoginKey, login);
            return RedirectToAction("Calendar", "Booking");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(SessionLoginKey);
            return RedirectToAction("Index", "Home");
        }
    }
}
