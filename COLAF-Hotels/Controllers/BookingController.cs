using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("Role"); // Get the role of the logged-in user
            var guest_id = HttpContext.Session.GetString("GuestId");

            List<Booking> bookings;

            if (userRole == "Staff" || userRole == "Admin")
            {
                // Staff/Admin can see all bookings
                bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .ThenInclude(g => g.User)
                    .Include(b => b.Room)
                    .ToList();
            }
            else if (int.TryParse(guest_id, out int guestId))
            {
                // Guests only see their own bookings
                bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Where(b => b.guest_id == guestId)
                    .ToList();
            }
            else
            {
                // No valid guest ID, return empty list
                bookings = new List<Booking>();
            }

            return View(bookings);
        }

        public IActionResult Details(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Guest)
                    .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public IActionResult Create(int roomId, string roomNumber, string roomImg, string roomCategory, decimal roomPrice)
        {
            var room = new Room
            {
                RoomId = roomId,
                RoomNumber = roomNumber,
                Category = roomCategory,
                ImageUrl = roomImg,
                Price = roomPrice
            };
            
            return View(room);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(string GuestId, string UserId, string RoomId, string RoomNumber, string Category, string ImageUrl, string Price, DateTime CheckInDate, DateTime CheckOutDate, decimal totalPrice)
        {
            Console.WriteLine($"Booking Confirmed: GuestId={GuestId}, UserId={UserId}, RoomId={RoomId}, CheckIn={CheckInDate}, CheckOut={CheckOutDate}, TotalPrice={totalPrice}");

            if (GuestId == "null")
            {
                // Add Guest for the user
                var newGuest = new Guest
                {
                    user_id = Convert.ToInt32(UserId)
                };
                _context.Guests.Add(newGuest);
                await _context.SaveChangesAsync();
                GuestId = newGuest.guest_id.ToString();
                Console.WriteLine($"Guest ID: {GuestId}");
            }

            CheckInDate = DateTime.SpecifyKind(CheckInDate, DateTimeKind.Utc);
            CheckOutDate = DateTime.SpecifyKind(CheckOutDate, DateTimeKind.Utc);

            // Create room object (you don't need to persist the room object, as it already exists in the DB)
            var room = new Room
            {
                RoomId = Convert.ToInt32(RoomId),
                RoomNumber = RoomNumber,
                Category = Category,
                ImageUrl = ImageUrl,
                Price = Convert.ToDecimal(Price)
            };

            // Validate Check-in and Check-out Dates
            if (CheckInDate < DateTime.UtcNow)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                return View("Create", room);
            }

            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                return View("Create", room);
            }

            var booking = new Booking
            {
                guest_id = Convert.ToInt32(GuestId),
                room_id = Convert.ToInt32(RoomId),
                check_in_date = CheckInDate,
                check_out_date = CheckOutDate,
                status = "Confirmed", // Options: Confirmed, Pending, Cancelled
                total_amount = totalPrice
            };

            // Find the guest and add the new booking to the guest's bookings list
            var guest = await _context.Guests.Include(g => g.Bookings)
                                             .FirstOrDefaultAsync(g => g.guest_id == Convert.ToInt32(GuestId));

            if (guest != null)
            {
                guest.Bookings.Add(booking);  // Add the booking to the guest's booking history
            }

            // Save both the booking and the updated guest to the database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your booking has been confirmed";
            TempData["BookingId"] = booking.booking_id;
            TempData["CheckInDate"] = booking.check_in_date.ToString("yyyy-MM-dd");
            TempData["CheckOutDate"] = booking.check_out_date.ToString("yyyy-MM-dd");
            TempData["TotalAmount"] = booking.total_amount.ToString();

            return RedirectToAction("Index", "Booking");
        }

        [HttpGet("GetUnavailableDates")]
        public IActionResult GetUnavailableDates(int roomId)
        {
            var unavailableDates = _context.Bookings
                .Where(b => b.room_id == roomId && b.status == "Confirmed")
                .AsEnumerable() // Move to memory processing
                .SelectMany(b => Enumerable.Range(0, (b.check_out_date - b.check_in_date).Days)
                    .Select(d => b.check_in_date.AddDays(d).ToString("yyyy-MM-dd")))
                .ToList();

            return Ok(unavailableDates);
        }


    }
}
