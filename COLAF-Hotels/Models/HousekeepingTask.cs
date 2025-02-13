namespace COLAFHotel.Models
{
    public class HousekeepingTask
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string TaskDescription { get; set; }
        public string Status { get; set; } // e.g., Pending, Completed
    }
}
