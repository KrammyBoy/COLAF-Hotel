using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLAFHotel.Models
{
    [Table("rooms")] // Ensure the table name matches your database schema
    public class Room
    {
        [Key]
        [Column("room_id")] // Use the correct column name
        public int RoomId { get; set; }

        [Required]
        [Column("room_number")]
        public string RoomNumber { get; set; }

        [Required]
        [Column("category")]
        public string Category { get; set; }  // e.g., Deluxe, Suite, Standard

        [Required]
        [Column("status")]
        public string Status { get; set; }    // Occupied, Vacant, Under Maintenance

        [Column("image_url")]
        public string ImageUrl { get; set; }

        [Column("image_type")] //Add new column in ROOM PostgreSQL
        public string? ImageType { get; set; } //This corresponds to the type of Image ( each category have two pictures)

        [Required]
        [Column("price")]
        public int Price { get; set; }

        [NotMapped]
        public int? Floor { get; set; }

    }
}
