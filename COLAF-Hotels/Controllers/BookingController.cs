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
            var bookings = _context.Bookings.Include(b => b.Guest).ToList(); // Ensure it retrieves data
            return View(bookings ?? new List<Booking>()); // Prevent null exception
        }

        public IActionResult Create(int roomId, string roomNumber, string roomImg, string roomCategory, int roomPrice)
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
            int guest_id = Convert.ToInt32(GuestId);
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
                guest_id = guest.guest_id;
                Console.WriteLine($"Guest ID: {guest_id}");
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
                guest_id = guest_id,
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
