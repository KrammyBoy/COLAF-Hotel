using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult SpecialServices()
        {
            // Hard-coded list of special services
            var services = new List<Service>
            {
                new Service { Name = "Spa Booking", Description = "Book spa appointments" },
                new Service { Name = "Airport Transfer", Description = "Arrange airport pickup/drop" },
                new Service { Name = "Room Service", Description = "Order room service" }
            };
            return View(services);
        }
    }
}
