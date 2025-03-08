using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Discount
    {
        [Key]
        public string discount_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public decimal discount_percentage { get; set; }
    }
}
