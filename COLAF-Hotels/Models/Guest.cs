using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace COLAFHotel.Models
{
    public class Guest
    {
        [Key]
        public int guest_id { get; set; }  // Primary Key

        public int? user_id { get; set; }

        public string? phone { get; set; }

        [ForeignKey("user_id")]
        public User User { get; set; }  // Navigation property
    }
}
