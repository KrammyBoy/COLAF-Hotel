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
            new Room { RoomNumber = "101", Category = "Deluxe", Status = "Vacant" },
            new Room { RoomNumber = "102", Category = "Suite", Status = "Occupied" },
            new Room { RoomNumber = "103", Category = "Standard", Status = "Under Maintenance" }
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
