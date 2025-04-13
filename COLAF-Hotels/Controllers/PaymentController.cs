using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using COLAFHotel.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace COLAFHotel.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ProcessPayment(int bookingId, string returnUrl)
        {
            var booking = _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefault(b => b.booking_id == bookingId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn", "CheckInCheckOut");
            }

            var payment = new Payment
            {
                booking_id = bookingId,
                Amount = (decimal)booking.totalBalance,
                ReturnUrl = returnUrl
            };

            return View(payment);
        }

        [HttpPost]
        public IActionResult ProcessPayment(Payment payment)
        {
            Console.WriteLine("Process Payment");
            if (!ModelState.IsValid)
            {
                return View(payment);
            }

            var booking = _context.Bookings.Find(payment.booking_id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn", "CheckInCheckOut");
            }

            // Process the payment
            booking.totalBalance -= payment.Amount;

            // Create payment record
            _context.Payments.Add(new Payment
            {
                booking_id = payment.booking_id,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                payment_date = DateTime.Now
            });

            _context.SaveChanges();

            // Set payment confirmation message
            ViewBag.Message = $"Payment of ₱{payment.Amount} processed successfully using {payment.PaymentMethod}.";

            // If a return URL was specified, store it for the confirmation page
            ViewBag.ReturnUrl = payment.ReturnUrl;

            return View("PaymentConfirmation", payment);
        }

        public IActionResult CompletePaymentProcess(int bookingId, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("CheckIn", "CheckInCheckOut");
            }

            return Redirect(returnUrl);
        }
    }
}