using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Data;
using COLAFHotel.Models;
using System.Linq;

namespace COLAFHotel.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            var room = _context.Room.ToList();  // Fetch data from PostgreSQL
            return View(room);
        }


        public IActionResult AdminRoom()
        {
            var room = _context.Room.ToList();  // Fetch data from PostgreSQL
            return View(room);
        }

        public IActionResult Details(string roomNumber)
        {
            var room = _context.Room.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost]
        public IActionResult UpdateRoom([FromBody] Room updatedRoom)
        {
            Console.WriteLine($"Received Room ID: {updatedRoom.RoomId}");
            var room = _context.Room.Find(updatedRoom.RoomId);
            if (room == null)
            {
                return NotFound();
            }

            // Update room properties
            room.ImageType = updatedRoom.ImageType;
            room.Category = updatedRoom.Category;
            room.Status = updatedRoom.Status;
            room.Price = updatedRoom.Price;

            room.ImageUrl = updatedImgUrl(room.Category, room.ImageType);
            // Update Image
         

            _context.Room.Update(room);
            _context.SaveChanges();

            return Json(new { success = true, message = "Room updated successfully!" });
        }
        public string updatedImgUrl(string category, string type)
        {
            string localcategory = category.ToLower();
            string imageUrl;
            if (category == "Deluxe")
            {
                imageUrl = (type == "alpha") ? $"~/assets/hotel_assets/hotel-{localcategory}.jpeg" : $"~/assets/hotel_assets/hotel-{localcategory}2.jpeg";
            }
            else
            {
                imageUrl = (type == "alpha") ? $"~/assets/hotel_assets/hotel-{localcategory}.jpg" : $"~/assets/hotel_assets/hotel-{localcategory}2.jpg";
            }
            return imageUrl;
        }
    }
}
