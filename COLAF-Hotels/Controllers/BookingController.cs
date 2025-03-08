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
            var guest_id = HttpContext.Session.GetString("GuestId");

            if (int.TryParse(guest_id, out int guestId)) // Ensure it's a valid integer
            {
                var bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .Where(b => b.guest_id == guestId) // Filter by GuestId
                    .ToList();

                return View(bookings);
            }

            return View(new List<Booking>()); // If GuestId is invalid, return an empty list

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
        public async Task<IActionResult> ConfirmBooking(string GuestId, string UserId, string RoomId, DateTime CheckInDate, DateTime CheckOutDate, decimal totalPrice)
        {
            
            Console.WriteLine($"Booking Confirmed: GuestId={GuestId}, UserId={UserId}, RoomId={RoomId}, CheckIn={CheckInDate}, CheckOut={CheckOutDate}, TotalPrice={totalPrice}");
            if (GuestId == "null")
            {
                // Add Guest for the user
                var guest = new Guest
                {
                    user_id = Convert.ToInt32(UserId)
                };
                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();
                GuestId = guest.guest_id.ToString();
                Console.WriteLine($"Guest ID: {GuestId}");
            }
            CheckInDate = DateTime.SpecifyKind(CheckInDate, DateTimeKind.Utc);
            CheckOutDate = DateTime.SpecifyKind(CheckOutDate, DateTimeKind.Utc);

            // Validate Check-in and Check-out Dates
            if (CheckInDate < DateTime.UtcNow)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                return RedirectToAction("Create", "Booking");
            }

            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                return RedirectToAction("Create", "Booking");
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
            //Confirmation
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Your booking has been confirmed";
            TempData["BookingId"] = booking.booking_id;
            TempData["CheckInDate"] = booking.check_in_date.ToString("yyyy-MM-dd");
            TempData["CheckOutDate"] = booking.check_out_date.ToString("yyyy-MM-dd");
            TempData["TotalAmount"] = booking.total_amount.ToString();
            return RedirectToAction("Index", "Booking");
        }
    }
}
