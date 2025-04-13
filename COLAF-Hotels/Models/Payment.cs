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
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime payment_date { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Booking Booking { get; set; }

        // Not stored in database, used for MVC flow
        [NotMapped]
        public string ReturnUrl { get; set; }
    }
}