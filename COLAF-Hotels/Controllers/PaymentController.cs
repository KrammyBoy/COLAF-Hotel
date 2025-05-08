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
                amount = (decimal)booking.totalBalance,
                ReturnUrl = returnUrl
            };

            Console.WriteLine($"Payment created - booking_id: {payment.booking_id}, Amount: {payment.amount}, ReturnUrl: {payment.ReturnUrl}");

            return View(payment);
        }

        [HttpPost]
        public IActionResult ProcessPayment(Payment payment)
        {

            ModelState.Remove("Booking");

            Console.WriteLine("Process Payment");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Valid Model: {ModelState.IsValid}");

                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ModelState Error - Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                return View(payment);
            }


            var booking = _context.Bookings.Find(payment.booking_id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("CheckIn", "CheckInCheckOut");
            }

            // Process the payment
            booking.totalBalance -= payment.amount   ;

            // Create payment record
            _context.Payments.Add(new Payment
            {
                booking_id = payment.booking_id,
                payment_method = payment.payment_method,
                amount = payment.amount,
                payment_date = DateTime.UtcNow
            });

            _context.SaveChanges();

            // Set payment confirmation message
            ViewBag.Message = $"Payment of ₱{payment.amount} processed successfully using {payment.payment_method}.";

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