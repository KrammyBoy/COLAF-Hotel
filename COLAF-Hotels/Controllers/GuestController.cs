using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class GuestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GuestController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Profile()
        {
            Console.WriteLine("Hello World");
            // Get current user's ID from session
            var guestIdString = HttpContext.Session.GetString("GuestId");
            Console.WriteLine($"Guest ID: {guestIdString}");
            var role = HttpContext.Session.GetString("Role");

            if (role != "Guest")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrEmpty(guestIdString) || !int.TryParse(guestIdString, out int guestId))
            {
                // Redirect to login if no valid guest ID in session
                Console.WriteLine("Diri ang error!");
                return RedirectToAction("Index", "Dashboard");
            }

            // Retrieve the complete guest information with related data
            var guest = await _context.Guests
                .Include(g => g.User)
                .Include(g => g.Bookings)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(g => g.guest_id == guestId);

            if (guest == null)
            {
                return RedirectToAction("Dashboard", "Index");
            }

            return View(guest);
        }
        public async Task<IActionResult> EditProfile()
        {
            // Get current user's ID from session
            var guestIdString = HttpContext.Session.GetString("GuestId");

            if (string.IsNullOrEmpty(guestIdString) || !int.TryParse(guestIdString, out int guestId))
            {
                // Redirect to login if no valid guest ID in session
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the guest information with user data
            var guest = await _context.Guests
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.guest_id == guestId);

            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }

        // POST: Guest/UpdateProfile
        // POST: Guest/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Guest guest, IFormFile ProfileImage)
        {
            if (guest == null)
            {
                return BadRequest();
            }

            // Get original guest and user data
            var originalGuest = await _context.Guests
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.guest_id == guest.guest_id);

            if (originalGuest == null)
            {
                return NotFound();
            }

            // Verify the current user is updating their own profile
            var guestIdString = HttpContext.Session.GetString("GuestId");
            if (string.IsNullOrEmpty(guestIdString) || !int.TryParse(guestIdString, out int currentGuestId) || currentGuestId != guest.guest_id)
            {
                return Forbid();
            }

            // Update user information, but preserve username and password
            originalGuest.User.firstname = guest.User.firstname;
            originalGuest.User.lastname = guest.User.lastname;
            originalGuest.User.email = guest.User.email;

            // Update profile image if provided
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(originalGuest.profile_image))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, originalGuest.profile_image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(fileStream);
                }

                originalGuest.profile_image = "/uploads/profiles/" + uniqueFileName;
            }

            // Update guest profile information
            originalGuest.date_of_birth = guest.date_of_birth.HasValue
    ? DateTime.SpecifyKind(guest.date_of_birth.Value, DateTimeKind.Utc)
    : (DateTime?)null;
            originalGuest.gender = guest.gender;
            originalGuest.pronouns = guest.pronouns;
            originalGuest.phone = guest.phone;
            originalGuest.mail_address = guest.mail_address;

            // Update stay preferences
            originalGuest.preferred_room_type = guest.preferred_room_type;
            originalGuest.location_pref = guest.location_pref;
            originalGuest.favorite_facilities = guest.favorite_facilities;
            originalGuest.wellness_preference = guest.wellness_preference;

            // Update health and dietary information
            originalGuest.dietary_restrictions = guest.dietary_restrictions;
            originalGuest.food_allergy = guest.food_allergy;
            originalGuest.medical_condition = guest.medical_condition;
            originalGuest.special_needs = guest.special_needs;

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect to profile page
            return RedirectToAction(nameof(Profile));
        }


        // List all guests for staff view
        public async Task<IActionResult> List()
        {
            // Check if user is staff or admin
            var userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Staff" && userRole != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Get guests with their user information
            var guests = await _context.Guests
                .Include(g => g.User)
                .Where(g => g.User.role == "Guest")
                .ToListAsync();

            return View(guests);
        }

        // Show guest details including profile, preferences, and history
        public async Task<IActionResult> Details(int id)
        {
            // Check if user is staff or admin
            var userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Staff" && userRole != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Get guest with user information and bookings
            var guest = await _context.Guests
                .Include(g => g.User)
                .Include(g => g.Bookings)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(g => g.guest_id == id);

            if (guest == null)
            {
                return NotFound();
            }

            // Calculate preferences based on booking history
            var guestViewModel = new GuestDetailsViewModel
            {
                Guest = guest,
                PreferredRoomType = GetPreferredRoomType(guest.Bookings),
                AverageStayLength = CalculateAverageStayLength(guest.Bookings),
                TotalBookings = guest.Bookings.Count,
                TotalSpent = guest.Bookings.Sum(b => b.total_amount)
            };

            return View(guestViewModel);
        }

        // Helper method to determine preferred room type
        private string GetPreferredRoomType(List<Booking> bookings)
        {
            if (bookings == null || !bookings.Any())
                return "No preference yet";

            return bookings
                .GroupBy(b => b.Room.Category)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        // Helper method to calculate average stay length
        private double CalculateAverageStayLength(List<Booking> bookings)
        {
            if (bookings == null || !bookings.Any())
                return 0;

            double totalDays = 0;
            foreach (var booking in bookings)
            {
                var days = (booking.check_out_date - booking.check_in_date).TotalDays;
                totalDays += days;
            }

            return Math.Round(totalDays / bookings.Count, 1);
        }
    }

    // View model for guest details page
    public class GuestDetailsViewModel
    {
        public Guest Guest { get; set; }
        public string PreferredRoomType { get; set; }
        public double AverageStayLength { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
