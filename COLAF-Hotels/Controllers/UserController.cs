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
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost("User/RegisterUser")] // Unique route for registration
        public async Task<IActionResult> Register(string username, string firstname, string lastname, string email, string password, string confirmPassword, string role)
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

            // Check if role is null or empty in the database
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == username);

            // Hash password before saving
            //string hashedPassword = HashPassword(password);

            if (existingUser != null)
            {
                role = existingUser.role; // Get role from DB if user exists
            }

            // If the role is still null or empty, assign "Guest"
            if (string.IsNullOrEmpty(role))
            {
                role = "Guest";
            }

            // Create user object
            User newUser = new User
            {
                username = username,
                firstname = firstname,
                lastname = lastname,
                email = email,
                password = password,
                //password = hashedPassword,
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
        public IActionResult RegisterPage()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.username == username);
            if (user == null || !(user.password == password))
            {
                ViewBag.Error = $"Invalid email or password.";
                return View();
            }
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("UserId", user.user_id.ToString());
            HttpContext.Session.SetString("User", user.username);
            HttpContext.Session.SetString("Role", user.role);
            /*
                Check if the user has guest_id.
                If it has guest_id, the user already interact with the system
                If not the booking system will create the guest_id for the user
             */
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.user_id == user.user_id);
            if (guest != null) {
                HttpContext.Session.SetString("GuestId", guest.guest_id.ToString());
            }else
            {
                HttpContext.Session.SetString("GuestId", "null");
            }
            Console.WriteLine($"Stored in Session - Username: {user.username}, Role: {HttpContext.Session.GetString("Role")}");
            return RedirectToAction("Index", "Dashboard");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ManageUsers() {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UserRoleUpdateModel model)
        {
            Console.WriteLine($"🔍 Received UpdateRole request - user_id: {model.user_id}, role: {model.role}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == model.user_id);

            if (user == null)
            {
                Console.WriteLine($"❌ ERROR: User with ID {model.user_id} not found!");
                return Json(new { message = $"User with ID {model.user_id} not found!" });
            }

            // Update the role in the database
            user.role = model.role;
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ SUCCESS: Role updated to {model.role}!");
            return Json(new { message = "Role updated successfully!" });
        }

        // ✅ Ensure this model matches JavaScript request
        public class UserRoleUpdateModel
        {
            public int user_id { get; set; } // ✅ Must match JavaScript key
            public string role { get; set; }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            Console.WriteLine($"🔍 Received DeleteUser request - user_id: {id}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == id);

            if (user == null)
            {
                Console.WriteLine($"❌ ERROR: User with ID {id} not found!");
                return Json(new { message = $"User with ID {id} not found!" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ SUCCESS: User with ID {id} deleted!");
            return Json(new { message = "User deleted successfully!" });
        }

    }
}
