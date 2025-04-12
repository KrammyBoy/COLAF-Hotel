using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Service
    {

        [Key]
        public int service_id { get; set; }

        [Required]
        public string name { get; set; }
        
        [Required]
        public string description { get; set; } //Description of the service

        [Required]
        public decimal price { get; set; }

        public ICollection<Booking_Service> BookingServices { get; set; }


    }
}
