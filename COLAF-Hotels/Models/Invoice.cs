using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Invoice
    {
        [Key]
        public int invoice_id { get; set; } // Primary Key

        [Required]
        public int booking_id { get; set; }

        [Required]
        public DateTime issue_date { get;set; } // Date when the invoice is issued


        [ForeignKey("booking_id")]
        public virtual Booking Booking { get; set; } // Navigation Property
    }
}
