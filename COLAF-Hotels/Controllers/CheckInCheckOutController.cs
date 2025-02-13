using Microsoft.AspNetCore.Mvc;

namespace COLAFHotel.Controllers
{
    public class CheckInCheckOutController : Controller
    {
        public IActionResult CheckIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckIn(string roomNumber, string guestName)
        {
            // Placeholder: In a real system, update room status to "Occupied"
            ViewBag.Message = $"Guest {guestName} checked into room {roomNumber}.";
            return View("CheckInConfirmation");
        }

        public IActionResult CheckOut()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckOut(string roomNumber, string guestName)
        {
            // Placeholder: Update room status to "Vacant"
            ViewBag.Message = $"Guest {guestName} checked out from room {roomNumber}.";
            return View("CheckOutConfirmation");
        }
    }
}
