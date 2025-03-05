using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;

namespace COLAFHotel.Controllers
{
    public class GuestController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult History()
        {
            // Placeholder for booking history
            return View();
        }
    }
}
