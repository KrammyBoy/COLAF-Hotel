using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class DiscountController : Controller
    {
        public IActionResult Offers()
        {
            // Hard-coded discount offers
            var offers = new List<Discount>
            {
                new Discount { Name = "Loyalty Discount", Description = "10% off for returning guests" },
                new Discount { Name = "Seasonal Offer", Description = "15% off during off-season" }
            };
            return View(offers);
        }
    }
}
