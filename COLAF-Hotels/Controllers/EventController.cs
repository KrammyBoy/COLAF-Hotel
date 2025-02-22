using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class EventController : Controller
    {
        public JsonResult GetEvents()
        {
            var events = new List<object>
            {
                new { title = "New Year Holiday", start = "2025-01-01" },
                new { title = "Company Anniversary", start = "2025-02-27" },
                new { title = "Team Meeting", start = "2025-02-10T10:00:00" }
            };
            return Json(events);
        }
    }
}
