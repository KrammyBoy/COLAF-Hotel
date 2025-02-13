namespace COLAFHotel.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string IssueDescription { get; set; }
        public string Status { get; set; } // Reported, In Progress, Completed
    }
}
