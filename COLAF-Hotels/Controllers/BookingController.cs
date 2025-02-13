using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class BookingController : Controller
    {
        // In-memory list of bookings
        private static List<Booking> bookings = new List<Booking>();

        public IActionResult Index()
        {
            return View(bookings);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Booking booking)
        {
            // For demo, assign an incremental ID and add to list.
            booking.Id = bookings.Count + 1;
            bookings.Add(booking);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var booking = bookings.Find(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
    }
}
