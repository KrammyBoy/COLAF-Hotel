using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COLAFHotel.Models;
using COLAFHotel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class HousekeepingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HousekeepingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Housekeeping/Tasks
        public async Task<IActionResult> Tasks(string filter = null)
        {
            ViewBag.CurrentFilter = filter;

            var query = _context.HousekeepingTasks
                .Include(t => t.Room)
                .AsQueryable();

            // Apply filters
            switch (filter)
            {
                case "pending":
                    query = query.Where(t => t.status == "Pending");
                    break;
                case "in-progress":
                    query = query.Where(t => t.status == "In Progress");
                    break;
                case "completed":
                    query = query.Where(t => t.status == "Completed");
                    break;
            }

            var tasks = await query.OrderByDescending(t => t.task_id).ToListAsync();

            // Get ALL user IDs that are referenced in tasks
            var userIds = tasks.Where(t => t.assigned_to.HasValue)
                               .Select(t => t.assigned_to.Value)
                               .Distinct()
                               .ToList();

            // Get staff names for display - getting all potentially referenced users
            var staffDict = new Dictionary<int, string>();

            var staff = await _context.Users
                          .Where(u => userIds.Contains(u.user_id)) // Get all users referenced in tasks
                          .ToListAsync();

            foreach (var employee in staff)
            {
                staffDict[employee.user_id] = employee.fullname;
            }

            // Now make sure that any missing IDs get a placeholder
            foreach (var id in userIds)
            {
                if (!staffDict.ContainsKey(id))
                {
                    staffDict[id] = $"User {id} (not found)";
                }
            }

            ViewBag.StaffNames = staffDict;

            return View(tasks);
        }

        // GET: /Housekeeping/TaskDetails/{id}
        public async Task<IActionResult> TaskDetails(int id)
        {
            var task = await _context.HousekeepingTasks
                .Include(t => t.Room)
                .FirstOrDefaultAsync(t => t.task_id == id);

            if (task == null)
            {
                return NotFound();
            }

 
            if (task.assigned_to.HasValue)
            {
                var staff = await _context.Users.FindAsync(task.assigned_to.Value);
                if (staff != null)
                {
                    ViewBag.StaffName = staff.fullname;
                }
            }

            return View(task);
        }

        // GET: /Housekeeping/CreateTask
        public async Task<IActionResult> CreateTask()
        {
            // Get available rooms for dropdown
            var rooms = await _context.Room.ToListAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber");

            // Get staff for dropdown
            var staff = await _context.Users
                                 .Where(u => u.role == "Housekeeper")
                                 .ToListAsync();

            ViewBag.Staff = new SelectList(staff, "user_id", "fullname");
            return View();
        }

        // POST: /Housekeeping/CreateTask
        [HttpPost]
        public async Task<IActionResult> CreateTask(HousekeepingTask task)
        {
            ModelState.Remove("Room");
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                task.status = "Pending";
                _context.Add(task);

                string room_status = "Vacant";
                if(task.task_type == "Room Cleaning")
                {
                    room_status = "Needs Cleaning";
                }
                else if(task.task_type != "Special Request")
                {
                    room_status = "Under Maintainance";
                }

                var room = await _context.Room.FindAsync(task.room_id.Value);

                if (room != null) {
                    room.Status = room_status;
                    _context.Update(room);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Task created successfully!";
                return RedirectToAction(nameof(Tasks));

            }

            // If we got here, something failed; redisplay form
            var rooms = await _context.Room.ToListAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", task.room_id);


            var staff = await _context.Users
                                      .Where(u => u.role == "Housekeeper")
                                      .ToListAsync();
            ViewBag.Staff = new SelectList(staff, "user_id", "fullname", task.assigned_to);

            return View(task);
        }

        // GET: /Housekeeping/EditTask/{id}
        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Get available rooms for dropdown
            var rooms = await _context.Room.ToListAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", task.room_id);

            // Get staff for dropdown - Using a safe approach
            var staff = await _context.Users
                                 .Where(u => u.role == "Housekeeper")
                                 .ToListAsync();

            ViewBag.Staff = new SelectList(staff, "user_id", "username", task.assigned_to);
            return View(task);
        }


        // POST: /Housekeeping/EditTask/{id}
        [HttpPost]
        public async Task<IActionResult> EditTask(int id, HousekeepingTask task)
        {
            ModelState.Remove("Room");
            ModelState.Remove("User");
            if (id != task.task_id)
            {
                return NotFound();
            }

            Console.WriteLine($"Valid Model: {ModelState.IsValid}");
            if (ModelState.IsValid)
            {
                try
                {
                    var oldTask = await _context.HousekeepingTasks.AsNoTracking().FirstOrDefaultAsync(t => t.task_id == id);
                    _context.Update(task);

                    // Check if the task status changed to Completed and it's a Room Cleaning task
                    if (oldTask.status != "Completed" && task.status == "Completed" && task.task_type == "Room Cleaning" && task.room_id.HasValue)
                    {
                        var room = await _context.Room.FindAsync(task.room_id.Value);
                        if (room != null)
                        {
                            room.Status = "Vacant";
                            _context.Update(room);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"Task #{id} updated successfully";
                    return RedirectToAction(nameof(Tasks));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.task_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got here, something failed; redisplay form
            var rooms = await _context.Room.ToListAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", task.room_id);

            var staff = await _context.Users
                                      .Where(u => u.role == "Housekeeper")
                                      .ToListAsync();
            ViewBag.Staff = new SelectList(staff, "user_id", "fullname", task.assigned_to);

            return View(task);
        }

        // POST: /Housekeeping/AcceptTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask(int taskId, int userId)
        {
            var task = await _context.HousekeepingTasks
                                     .Include(t => t.User) // Eager load the related User
                                     .FirstOrDefaultAsync(t => t.task_id == taskId);
            if (task == null)
            {
                return NotFound();
            }

            task.status = "In Progress";
            task.assigned_to = userId;

            _context.Update(task);
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{task.User.firstname} {task.User.lastname} accepted Task #{task.task_id} and marked as {task.status}";
            return RedirectToAction(nameof(Tasks));
        }

        // POST: /Housekeeping/CompleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var task = await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            task.status = "Completed";

            // If this is a room cleaning task, update the room status to Clean
            if (task.task_type == "Room Cleaning" && task.room_id.HasValue)
            {
                var room = await _context.Room.FindAsync(task.room_id.Value);
                if (room != null)
                {
                    room.Status = "Vacant";
                    _context.Update(room);
                }
            }

            _context.Update(task);
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{task.User.firstname} {task.User.lastname} completed Task #{task.task_id} and marked as {task.status}";
            return RedirectToAction(nameof(Tasks));
        }

        private bool TaskExists(int id)
        {
            return _context.HousekeepingTasks.Any(e => e.task_id == id);
        }
    }
}