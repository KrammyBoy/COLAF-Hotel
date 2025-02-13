using Microsoft.AspNetCore.Mvc;

namespace COLAFHotel.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Landing page after login
            return View();
        }
    }
}
