using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;

namespace COLAFHotel.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult ProcessPayment()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(Payment payment)
        {
            // Hard-coded payment processing (always successful for demo)
            ViewBag.Message = "Payment processed successfully. Receipt generated.";
            return View("PaymentConfirmation", payment);
        }
    }
}
