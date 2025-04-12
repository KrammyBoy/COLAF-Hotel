using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult SpecialServices()
        {
            return View();
        }
    }
}
