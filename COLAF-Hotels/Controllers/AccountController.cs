using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
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
        public IActionResult Login(string username, string password)
        {
            // Hard-coded credentials: username: "test", password: "1234"
            if (username == "test" && password == "1234")
            {
                // Using TempData to simulate session login
                TempData["User"] = username;
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public IActionResult Register()
        {
            // Placeholder registration view
            return View();
        }

        public IActionResult ManageUsers()
        {
            // Hard-coded list of users for demo
            var users = new List<string> { "test", "user1", "user2" };
            return View(users);
        }

        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}
