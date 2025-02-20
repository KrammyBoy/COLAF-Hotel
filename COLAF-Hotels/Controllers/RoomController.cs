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

        public IActionResult Details(string roomNumber)
        {
            var room = _context.Room.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }
    }
}
