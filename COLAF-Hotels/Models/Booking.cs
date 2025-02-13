using System;

namespace COLAFHotel.Models
{
    public class Booking
    {
        public int Id { get; set; } // In-memory identifier
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } // Confirmed, Pending, etc.
    }
}
