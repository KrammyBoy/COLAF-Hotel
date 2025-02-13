using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;

namespace COLAFHotel.Controllers
{
    public class GuestController : Controller
    {
        public IActionResult Profile()
        {
            // Hard-coded guest info for demo
            var guest = new Guest
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "123-456-7890"
            };
            return View(guest);
        }

        public IActionResult History()
        {
            // Placeholder for booking history
            return View();
        }
    }
}
