using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Booking
    {
        [Key]
        public int booking_id { get; set; } // Primary Key

        [Required]
        public int guest_id { get; set; }

        [Required]
        public int room_id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime check_in_date { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime check_out_date { get; set; }

        [Required]
        [StringLength(50)]
        public string status { get; set; } // Confirmed, Pending, Cancelled, etc.

        [Required]
        public decimal total_amount { get; set; }

        [Required]
        [Column("totalBalance")]
        public decimal? totalBalance { get; set; } // Balance: Check if the guest has paid the total amount or not

        public int? discount_id { get; set; }

        public string? guestName { get; set; }

        // Foreign Key - Relationship with Guest
        [ForeignKey("guest_id")]

        public virtual Guest Guest { get; set; } // Navigation Property

        [ForeignKey("room_id")]
        public virtual Room Room { get; set; } // Navigation Property
    }
}
