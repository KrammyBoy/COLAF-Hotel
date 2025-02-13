namespace COLAFHotel.Models
{
    public class Room
    {
        public string RoomNumber { get; set; }
        public string Category { get; set; }  // e.g., Deluxe, Suite, Standard
        public string Status { get; set; }    // Occupied, Vacant, Under Maintenance
    }
}
