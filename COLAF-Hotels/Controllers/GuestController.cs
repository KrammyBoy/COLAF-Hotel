using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using COLAFHotel.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class GuestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            return View();
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
