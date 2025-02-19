using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class RoomController : Controller
    {
        // In-memory list of rooms (to be replaced with a DB later)
        private static List<Room> rooms = new List<Room>
        {
            new Room { RoomNumber = "101", Category = "Deluxe", Status = "Vacant", ImageUrl = "~/assets/hotel_assets/hotel-deluxe.jpeg", Offerings = "Free breakfast", Price = 5000 },
            new Room { RoomNumber = "102", Category = "Suite", Status = "Occupied", ImageUrl = "~/assets/hotel_assets/hotel-suite.jpg", Offerings = "Free breakfast" , Price = 3500 },
            new Room { RoomNumber = "103", Category = "Standard", Status = "Under Maintenance" , ImageUrl = "~/assets/hotel_assets/hotel-standard.jpg", Offerings = "Free breakfast", Price = 1800 },
            new Room { RoomNumber = "603", Category = "Deluxe", Status = "Vacant" , ImageUrl = "~/assets/hotel_assets/hotel-deluxe2.jpeg", Offerings = "Free breakfast", Price = 7800 }
        };

        public IActionResult List()
        {
            return View(rooms);
        }

        public IActionResult Details(string roomNumber)
        {
            var room = rooms.Find(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }
    }
}
