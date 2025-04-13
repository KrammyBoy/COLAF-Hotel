using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Payment
    {
        [Key]
        public int payment_id { get; set; }

        public int? booking_id { get; set; }

        [StringLength(50)]
        public string payment_method { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal amount { get; set; }

        public DateTime payment_date { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("booking_id")]
        public Booking Booking { get; set; }

        // Not stored in database, used for MVC flow
        [NotMapped]
        public string ReturnUrl { get; set; }
    }
}