using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Npgsql;


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int request_id {  get; set; } // Primary Key 
    
        public int room_id { get; set; }

        public string issue_description { get; set; } // Description of the maintenance issue
        
        public string status { get; set; } // Status of the request (e.g., Pending, In Progress, Completed)
        public DateTime reported_date { get; set; } // Date when the request was made    
        public DateTime? resolved_date { get; set; } // Date when the issue was resolved (nullable)

        public string? assigned { get; set; } // Name of the staff/housekeeper member assigned to the request

        [ForeignKey("room_id")]
        public Room Room { get; set; } // Navigation property to the Room entity
    }
}
