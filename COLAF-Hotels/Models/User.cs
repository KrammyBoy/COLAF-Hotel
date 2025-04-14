using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    public class User
    {
        [Key]
        public int user_id { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        public string email { get; set; }
        [Required]
        public string firstname { get; set; }
        [Required]
        public string lastname { get; set; }
        public string? role { get; set; } //Guest, Staff, Admin, Housekeeper

        [NotMapped]
        public string fullname => $"{firstname} {lastname}";
        [NotMapped]
        public string? profile_image_alt => $"{firstname.Substring(0, 1)}{lastname.Substring(0, 1)}".ToUpper();
    }
}
