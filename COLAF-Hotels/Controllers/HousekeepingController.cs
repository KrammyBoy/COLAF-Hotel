using Microsoft.AspNetCore.Mvc;
using COLAFHotel.Models;
using System.Collections.Generic;

namespace COLAFHotel.Controllers
{
    public class HousekeepingController : Controller
    {
        // In-memory list of housekeeping tasks
        private static List<HousekeepingTask> tasks = new List<HousekeepingTask>();

        public IActionResult Tasks()
        {
            return View(tasks);
        }

        public IActionResult CreateTask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTask(HousekeepingTask task)
        {
            task.task_id = tasks.Count + 1;
            tasks.Add(task);
            return RedirectToAction("Tasks");
        }
    }
}
