using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Notification
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int notification_id { get; set; }

        [ForeignKey("User")] // Specifies that user_id is a foreign key to the User table.
        public int? user_id { get; set; }

        [Required] // The message field cannot be null.
        public string message { get; set; }

        public DateTime sent_date { get; set; } = DateTime.Now; // Default to current timestamp.

        public bool read_status { get; set; } = false; // Default to false (not read).

        // Navigation property to represent the relationship with the User model.
        public virtual User User { get; set; }
    }
}
