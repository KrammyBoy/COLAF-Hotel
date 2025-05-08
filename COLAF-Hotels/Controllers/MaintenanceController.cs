using Azure.Core;
using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Requests()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int UserID))
            {
                return BadRequest("Invalid User ID in session.");
            }

            List<MaintenanceRequest> requests;

            if (role == "Guest")
            {
                requests = await _context.MaintenanceRequests
                    .Include(r => r.Room)
                    .Where(r => _context.Bookings
                        .Any(b => b.room_id == r.room_id &&
                                  _context.Guests.Any(g => g.guest_id == b.guest_id && g.user_id == UserID)))
                    .ToListAsync();
            }
            else if (role == "Housekeeper")
            {
                requests = await _context.MaintenanceRequests
                    .Include(r => r.Room)
                    .ToListAsync();
            }
            else
            {
                return Forbid(); // Optional: handle other roles
            }

            return View(requests);
        }

        public async Task<IActionResult> CreateRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var guestId = HttpContext.Session.GetString("GuestId");

            if (!int.TryParse(userId, out int UserID))
            {
                return BadRequest("Invalid User ID in session.");
            }

            if (!int.TryParse(guestId, out int GuestID))
            {
                return BadRequest("Invalid Guest ID in session.");
            }

            Console.WriteLine($"Creating Request: UserId = {UserID} GuestID = {GuestID}");

            var availableRooms = await _context.Bookings
                .Where(b => b.guest_id == GuestID && b.status == "Checked In")
                .Join(_context.Room,
                    booking => booking.room_id,
                    room => room.RoomId,
                    (booking, room) => new SelectListItem
                    {
                        Value = room.RoomId.ToString(),
                        Text = room.RoomNumber
                    })
                .Distinct()
                .ToListAsync();

            ViewBag.RoomList = availableRooms;

            foreach (var room in availableRooms)
            {
                Console.WriteLine($"RoomId: {room.Value}, RoomNumber: {room.Text}");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(MaintenanceRequest request)
        {

            ModelState.Remove("request_id");
            ModelState.Remove("resolved_date");
            ModelState.Remove("assigned");
            ModelState.Remove("Room");
            ModelState.Remove("status");

            request.reported_date = DateTime.UtcNow;
            request.status = "Pending";
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
            }

            // Log the request object details to the console
            Console.WriteLine("Creating Maintenance Request:");
            Console.WriteLine($"Room ID: {request.room_id}");
            Console.WriteLine($"Issue Description: {request.issue_description}");
            Console.WriteLine($"Status: {request.status}");
            Console.WriteLine($"Reported Date: {request.reported_date}");

            var guestId = HttpContext.Session.GetString("GuestId");
            if (!int.TryParse(guestId, out int GuestID))
            {
                Console.WriteLine("Bad Request");
                return BadRequest("Invalid Guest ID in session.");
            }

            if (ModelState.IsValid) // Fixed logic error: changed from "!ModelState.IsValid" to "ModelState.IsValid"
            {
                Console.WriteLine("Model Valid");
                await _context.MaintenanceRequests.AddAsync(request);
                await _context.SaveChangesAsync();

                // Clear ViewBag after creating the request
                ViewBag.RoomList = null;

                // Redirect to the "Requests" page after successfully creating the request
                TempData["Notification"] = $"Request #{request.request_id} were given to the housekeeper";
                TempData["NotificationType"] = "success";

                return RedirectToAction("Requests");
            }
            // If model state is invalid, redisplay the form
            var availableRooms = await _context.Bookings
                .Where(b => b.guest_id == GuestID && b.status == "Checked In")
                .Join(_context.Room,
                    booking => booking.room_id,
                    room => room.RoomId,
                    (booking, room) => new SelectListItem
                    {
                        Value = room.RoomId.ToString(),
                        Text = room.RoomNumber
                    })
                .Distinct()
                .ToListAsync();

            ViewBag.RoomList = availableRooms;

            return View(request);
        }

        // Added missing action methods referenced in the view

        public async Task<IActionResult> Accept(int id)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User not found in session.");
            }

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Update the request status and assign it to the current user
            request.status = "In Progress";
            request.assigned = user;

            await _context.SaveChangesAsync();

            TempData["Notification"] = $"{request.request_id} successfully assigned to {request.assigned}";
            TempData["NotificationType"] = "success";

            CreateMaintenanceNotificationAsync(id, "Accept").Wait();

            return RedirectToAction("Requests");
        }

        public async Task<IActionResult> Complete(int id)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User not found in session.");
            }

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Check if the request is assigned to the current user
            if (request.assigned != user)
            {
                TempData["ErrorMessage"] = "You can only complete requests assigned to you.";
                return RedirectToAction("Requests");
            }

            // Update the request status and set the resolved date
            request.status = "Completed";
            request.resolved_date = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Notification"] = $"Request #{request.request_id} was successfully completed by {request.assigned}";
            TempData["NotificationType"] = "success";

            CreateMaintenanceNotificationAsync(id, "Complete").Wait();

            return RedirectToAction("Requests");
        }

        public async Task<IActionResult> Edit(int id)
        {

            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User not found in session.");
            }

            var request = await _context.MaintenanceRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.request_id == id);

            if (request == null)
            {
                return NotFound();
            }

            // Check if the request is assigned to the current user
            if (request.assigned != user)
            {
                TempData["ErrorMessage"] = "You can only edit requests assigned to you.";
                return RedirectToAction("Requests");
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRequest request)
        {
            ModelState.Remove("resolved_date");
            if (id != request.request_id)
            {
                return NotFound();
            }

            var user = HttpContext.Session.GetString("User");
            var existingRequest = await _context.MaintenanceRequests.FindAsync(id);

            if (existingRequest == null)
            {
                return NotFound();
            }

            // Check if the request is assigned to the current user
            if (existingRequest.assigned != user)
            {
                TempData["ErrorMessage"] = "You can only edit requests assigned to you.";
                return RedirectToAction("Requests");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Only update specific fields to prevent overwriting critical data
                    existingRequest.issue_description = request.issue_description;

                    // Update status only if it's changed and is valid
                    if (request.status != existingRequest.status &&
                        (request.status == "In Progress" || request.status == "Completed"))
                    {
                        existingRequest.status = request.status;

                        // If status is set to Completed, set the resolved date
                        if (request.status == "Completed")
                        {
                            existingRequest.resolved_date = DateTime.UtcNow;

                            
                        }
                    }

                    await _context.SaveChangesAsync();

                    CreateMaintenanceNotificationAsync(id, "Complete").Wait();

                    return RedirectToAction("Requests");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRequestExists(request.request_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(request);
        }

        public async Task CreateMaintenanceNotificationAsync(int maintenanceRequestId, string action)
        {
            // Get the maintenance request with Room information
            var request = await _context.MaintenanceRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.request_id == maintenanceRequestId);

            if (request == null)
                return;

            // Find the booking for this room to identify the guest
            var activeBooking = await _context.Bookings
                .Include(b => b.Guest)
                .ThenInclude(g => g.User)
                .FirstOrDefaultAsync(b =>
                    b.room_id == request.room_id &&
                    b.status == "Checked In");

            if (activeBooking?.Guest?.User == null)
                return;

            // Create appropriate message based on action
            string message = action switch
            {
                "Accept" => $"Your request for Room {request.Room.RoomNumber} has been accepted by {request.assigned} and is now in progress.",
                "Complete" => $"Good news! Your request for Room {request.Room.RoomNumber} has been deemed complete. Thank you for your patience",
                _ => $"Update on your maintenance request for Room {request.Room.RoomNumber}."
            };

            // Create notification for the guest
            var notification = new Notification
            {
                user_id = activeBooking.Guest.user_id,
                message = message,
                sent_date = DateTime.UtcNow,
                read_status = false
            };

            // Save to database
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.request_id == id);
        }
    }
}