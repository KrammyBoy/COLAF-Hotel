using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class HousekeepingTask
    {
        [Key]
        public int task_id { get; set; }

        public int? room_id { get; set; }

        public string? task_type { get; set; } // e.g., Room Cleaning, Facility Maintenance, add if needed

        [Required]
        public string description { get; set; }

        public string status { get; set; } = "Pending"; // Pending, In Progress, Completed

        public int? assigned_to { get; set; } //IDK why is this int but maybe if we have a employee table??

        // Navigation properties
        [ForeignKey("room_id")]
        public virtual Room Room { get; set; }

        [ForeignKey("assigned_to")]
        public virtual User User { get; set; }
    }
}