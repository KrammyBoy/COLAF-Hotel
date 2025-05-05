using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class Discount
    {
        [Key]
        public int discount_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal discount_value { get; set; }

        [Required]
        public string status { get; set; }

        [Required]
        public DateTime expiration_date { get; set; }

        [Required]
        public string title { get; set; }

        [Required]

        public string promo_code { get; set; }

    }
}
