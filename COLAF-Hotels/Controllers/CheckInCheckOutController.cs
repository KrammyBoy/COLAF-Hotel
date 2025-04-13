using COLAFHotel.Data;
using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class CheckInCheckOutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInCheckOutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CheckIn()
        {
            var userRole = HttpContext.Session.GetString("Role");
            var guest_id = HttpContext.Session.GetString("GuestId");

            List<Booking> bookings;

            if (userRole == "Staff" || userRole == "Admin")
            {
                // Staff/Admin can see all bookings
                bookings = _context.Bookings
                    .Include(b => b.Guest)
                    .ThenInclude(g => g.User)
                    .Include(b => b.Room)
                    .OrderBy(b => b.check_in_date)
                    .ToList();
            }
            else
            {
                // No valid guest ID, return empty list
                bookings = new List<Booking>();
            }

            return View(bookings);
        }

        [HttpGet]
        public IActionResult CancelAllExpiredReservation()
        {
            var today = DateTime.UtcNow.Date;

            // 1. Cancel Confirmed bookings that are more than 2 days past check-in
            var expiredConfirmed = _context.Bookings
                .Where(b => b.check_in_date.Date < today.AddDays(-2) && b.status == "Confirmed")
                .ToList();

            // 2. Cancel Pending bookings that are today or earlier
            var stalePending = _context.Bookings
                .Where(b => b.check_in_date.Date <= today && b.status == "Pending")
                .ToList();

            foreach (var booking in expiredConfirmed.Concat(stalePending))
            {
                booking.status = "Cancelled";
                // Prepare for future notification
                // CreateNotification(booking.guest_id, $"Your booking #{booking.booking_id} has been automatically cancelled due to expiration.");
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"{expiredConfirmed.Count + stalePending.Count} expired or stale pending bookings were cancelled.";
            return RedirectToAction("CheckIn");
        }

        [HttpGet]
        public IActionResult ProcessCheckIn(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Guest)
                .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            // Check if guest has a balance
            if (booking.totalBalance > 0)
            {
                // Guest has a balance, redirect to payment option page
                return View("CheckInPaymentOptions", booking);
            }

            // No balance, proceed with check-in
            return ProcessCheckInConfirm(id, false);
        }

        [HttpPost]
        public IActionResult ProcessCheckInConfirm(int id, bool payNow)
        {
            var booking = _context.Bookings
                .Include(b => b.Guest)
                .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            if (payNow)
            {
                // Redirect to payment page with return URL to this action
                TempData["ReturnAction"] = "ProcessCheckInAfterPayment";
                TempData["BookingId"] = id;
                return RedirectToAction("ProcessPayment", "Payment", new { bookingId = id, returnUrl = Url.Action("ProcessCheckInAfterPayment", "CheckInCheckOut", new { id }) });
            }

            // Update booking status
            booking.status = "Checked In";

            // Update room status
            var room = _context.Room.Find(booking.room_id);
            if (room != null)
            {
                room.Status = "Occupied";
            }

            _context.SaveChanges();

            // Prepare for future notification
            // CreateNotification(booking.guest_id, $"You have successfully checked in to Room {booking.Room.RoomNumber}. Enjoy your stay!");

            TempData["SuccessMessage"] = $"Guest successfully checked in to Room {booking.Room.RoomNumber}.";
            return RedirectToAction("CheckIn");
        }

        public IActionResult ProcessCheckInAfterPayment(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            // Update booking status
            booking.status = "Checked In";
            booking.totalBalance = 0; // Payment completed

            // Update room status
            var room = _context.Room.Find(booking.room_id);
            if (room != null)
            {
                room.Status = "Occupied";
            }

            _context.SaveChanges();

            // Prepare for future notification
            // CreateNotification(booking.guest_id, $"You have successfully checked in to Room {booking.Room.RoomNumber} and your payment has been processed. Enjoy your stay!");

            TempData["SuccessMessage"] = $"Payment processed and guest successfully checked in to Room {booking.Room.RoomNumber}.";
            return RedirectToAction("CheckIn");
        }

        [HttpGet]
        public IActionResult ProcessCheckOut(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Guest)
                .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            // Check if guest has a balance
            if (booking.totalBalance > 0)
            {
                // Guest has a balance, must pay before checking out
                TempData["ReturnAction"] = "ProcessCheckOutAfterPayment";
                TempData["BookingId"] = id;
                return RedirectToAction("ProcessPayment", "Payment", new { bookingId = id, returnUrl = Url.Action("ProcessCheckOutAfterPayment", "CheckInCheckOut", new { id }) });
            }

            // No balance, proceed with check-out
            return ProcessCheckOutConfirm(id);
        }

        [HttpPost]
        public IActionResult ProcessCheckOutConfirm(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Guest)
                .ThenInclude(g => g.User)
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            // Update booking status
            booking.status = "Checked Out";

            // Update room status and schedule housekeeping
            var room = _context.Room.Find(booking.room_id);
            if (room != null)
            {
                room.Status = "Needs Cleaning";

                // Create housekeeping task
                _context.HousekeepingTasks.Add(new HousekeepingTask
                {
                    room_id = room.RoomId,
                    description = $"Room cleaning after checkout from booking #{booking.booking_id}",
                    status = "Pending"
                });
            }

            _context.SaveChanges();

            // Prepare for future notification
            // CreateNotification(booking.guest_id, $"You have successfully checked out from Room {booking.Room.RoomNumber}. Thank you for staying with us!");

            TempData["SuccessMessage"] = $"Guest successfully checked out from Room {booking.Room.RoomNumber}.";
            return RedirectToAction("CheckIn");
        }

        public IActionResult ProcessCheckOutAfterPayment(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn");
            }

            // Update booking status
            booking.status = "Checked Out";
            booking.totalBalance = 0; // Payment completed

            // Update room status and schedule housekeeping
            var room = _context.Room.Find(booking.room_id);
            if (room != null)
            {
                room.Status = "Needs Cleaning";

                // Create housekeeping task
                _context.HousekeepingTasks.Add(new HousekeepingTask
                {
                    room_id = room.RoomId,
                    description = $"Room cleaning after checkout from booking #{booking.booking_id}",
                    status = "Pending"
                });
            }

            _context.SaveChanges();

            // Prepare for future notification
            // CreateNotification(booking.guest_id, $"You have successfully checked out from Room {booking.Room.RoomNumber} and your payment has been processed. Thank you for staying with us!");

            TempData["SuccessMessage"] = $"Payment processed and guest successfully checked out from Room {room.RoomNumber}.";
            return RedirectToAction("CheckIn");
        }

        // This method is commented out as it's for future implementation
        /*
        private void CreateNotification(int? userId, string message)
        {
            if (userId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    user_id = userId.Value,
                    message = message,
                    sent_date = DateTime.Now,
                    read_status = false
                });
                _context.SaveChanges();
            }
        }
        */
    }
}