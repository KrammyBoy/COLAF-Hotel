using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class DiscountController : Controller
    {
        public IActionResult Offers()
        {
            return View();
        }
    }
}
