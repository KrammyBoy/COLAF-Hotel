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
