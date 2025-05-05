using COLAFHotel.Data;
using COLAFHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace COLAFHotel.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var userIdString = HttpContext.Session.GetString("UserId");
            Console.WriteLine($"User Id: {userIdString}");

            if (role == null || userIdString == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Convert userIdString to integer
            if (int.TryParse(userIdString, out var userId))
            {
                // Get notifications for the user
                var notifications = _context.Notifications
                                            .Where(n => n.user_id == userId)
                                            .OrderByDescending(n => n.sent_date)
                                            .ToList();

                return View(notifications); // Pass the list of notifications to the view
            }

            return RedirectToAction("Login", "Account"); // Redirect if parsing fails
        }

        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            Console.WriteLine("Getting unread notification count");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { count = 0 }); // If no user is logged in, return 0
            }

            // Convert userIdString to integer
            if (int.TryParse(userIdString, out var userId))
            {
                // Count unread notifications for the specific user
                var unreadNotificationsCount = _context.Notifications
                                                      .Where(n => n.user_id == userId && !n.read_status)
                                                      .Count();

                return Json(new { count = unreadNotificationsCount });
            }

            return Json(new { count = 0 }); // Return 0 if there's any issue with the userId
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notification_id, string returnUrl = null)
        {
            // Get current user
            var userIdString = HttpContext.Session.GetString("UserId");

            // Convert to int if your user_id is int in the database
            if (int.TryParse(userIdString, out int userIdInt))
            {
                // Find the notification in the database asynchronously
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.notification_id == notification_id && n.user_id == userIdInt);

                if (notification != null)
                {
                    // Mark as read
                    notification.read_status = true;
                    await _context.SaveChangesAsync(); // Save changes asynchronously
                }
            }

            // Redirect back to the page they were on
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Or redirect to a default page
            return RedirectToAction("Index", "Dashboard");
        }


        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var unreadNotifications = await _context.Notifications
                                                  .Where(n => n.user_id == userId && !n.read_status)
                                                  .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.read_status = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                                           .FirstOrDefaultAsync(n => n.notification_id == id && n.user_id == userId);

            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}