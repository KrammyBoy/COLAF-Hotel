using COLAFHotel.Data;
using Microsoft.AspNetCore.Mvc;

namespace COLAFHotel.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext through the constructor
        public DashboardController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList(); // Fetch users from the database
            return View(users);
        }
    }
}
