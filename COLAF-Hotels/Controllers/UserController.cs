using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using COLAFHotel.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace COLAFHotel.Controllers
{
    public class UserController : Controller
    {
        string role;

        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string firstname, string lastname, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            if (_context.Users.Any(u => u.username == username))
            {
                ViewBag.Error = "Username already exists!";
                return View();
            }

            // Hash password before saving
            string hashedPassword = HashPassword(password);

            if (role == null || role == "") role = "Guest";

            // Create user object
            User newUser = new User
            {
                username = username,
                firstname = firstname,
                lastname = lastname,
                email = email,
                password = hashedPassword,
                role = role
            };

            // Save user to database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            Console.WriteLine($"Received Username: {username}");
            Console.WriteLine($"Received Password: {password}");

            // Find user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.username == username);

            if (user == null || !(user.password == password))
            {
                ViewBag.Error = $"Invalid email or password.";
                return View();
            }

            // Store user session
            HttpContext.Session.SetString("UserId", user.user_id.ToString());
            HttpContext.Session.SetString("User", user.username);
            HttpContext.Session.SetString("Role", user.role);

            // Redirect to dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // Password Hashing
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync(); // Fetch users from DB
            return PartialView("~/Views/User/ManageUsers.cshtml", users); // Return partial view
        }
    }
}
