namespace COLAFHotel.Models
{
    public class Room
    {
        public string RoomNumber { get; set; }
        public string Category { get; set; }  // e.g., Deluxe, Suite, Standard
        public string Status { get; set; }    // Occupied, Vacant, Under Maintenance

        public string ImageUrl { get; set; }

        public int Price { get; set; }
        public string Offerings { get; set; } // Free breakfast, free lunch, buffet, etc etc
    }
}
