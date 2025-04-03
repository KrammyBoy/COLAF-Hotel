using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
            // Get the role from the session
            var userRole = HttpContext.Session.GetString("Role");

            // Pass the role to the view
            ViewBag.UserRole = userRole;

            var room = _context.Room.ToList();  // Fetch data from PostgreSQL

            return View(room);
        }


        public IActionResult AdminRoom()
        {
            var room = _context.Room.ToList();  // Fetch data from PostgreSQL
            return View(room);
        }

        public IActionResult CreateRoom()
        {
            return View();
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

        [HttpPost]
        public IActionResult Create(Room newRoom)
        {
            ModelState.Remove(nameof(newRoom.ImageUrl));
            newRoom.RoomNumber = ((Convert.ToInt32(newRoom.Floor) * 100) + Convert.ToInt32(newRoom.RoomNumber)).ToString();
            newRoom.ImageUrl = updatedImgUrl(newRoom.Category, newRoom.ImageType);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View("CreateRoom", newRoom);
            }

            // Add the new room to the database
            _context.Room.Add(newRoom);
            _context.SaveChanges();

            // Redirect to AdminRoom or another appropriate page
            return RedirectToAction("AdminRoom");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            Console.WriteLine($"🔍 Received DeleteRoom request - room_id: {id}");

            var room = await _context.Room.FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                Console.WriteLine($"❌ ERROR: Room with ID {id} not found!");
                return NotFound(new { success = false, message = $"Room with ID {id} not found!" });
            }

            _context.Room.Remove(room);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ SUCCESS: Room with ID {id} deleted!");

            return Json(new { success = true, message = "Room deleted successfully!" });
        }




    }
}
