using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class MaintenanceController : Controller
    {
        // In-memory list of maintenance requests
        private static List<MaintenanceRequest> requests = new List<MaintenanceRequest>();

        public IActionResult Requests()
        {
            return View(requests);
        }

        public IActionResult CreateRequest()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateRequest(MaintenanceRequest request)
        {
            request.Id = requests.Count + 1;
            requests.Add(request);
            return RedirectToAction("Requests");
        }
    }
}
