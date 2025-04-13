using System;
using System.ComponentModel.DataAnnotations;

namespace COLAFHotel.Models
{
    public class HousekeepingTask
    {
        [Key]
        public int task_id { get; set; }

        public int? room_id { get; set; }

        [Required]
        public string description { get; set; }

        public string status { get; set; } = "Pending"; // Pending, In Progress, Completed

        public int? assigned_to { get; set; } //IDK why is this int but maybe if we have a employee table??

        // Navigation properties
        public virtual Room Room { get; set; }
    }
}