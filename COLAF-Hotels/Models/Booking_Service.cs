using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Booking_Service
    {
        public int booking_id { get; set; } // Foreign key to Booking table

        public int service_id { get; set; } // Foreign key to Service table

        public Booking Booking { get; set; }
        public Service Service { get; set; }
    }
}
