using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            // Hard-coded list of notifications for demo
            var notifications = new List<string>
            {
                "Booking confirmation sent to guest@example.com",
                "Room 101 is now under maintenance",
                "Special offer: 15% off this season!"
            };
            return View(notifications);
        }
    }
}
