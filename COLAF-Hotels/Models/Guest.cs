using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace COLAFHotel.Models
{
    public class Guest
    {
        [Key]
        public int guest_id { get; set; }  // Primary Key

        public int? user_id { get; set; }

        //General Standard Information
        public string? phone { get; set; }
        public string? profile_image { get; set; } // The standard url for the profile image is ~/assets/profile_image/U{User.user_id}_profile.jpg
        public string? mail_address { get; set; }
        public DateTime? date_of_birth { get; set; } // Very useful for birthday discounts and offers
        public string? gender { get; set; }
        public string? pronouns { get; set; }

        // Stay Preferences
        public string? preferred_room_type { get; set; } // e.g. Suite, Deluxe, Standard
        public string? favorite_facilities { get; set; }
        public string? location_pref { get; set; } // e.g. High Floor, Low Floor, Near Elevator, Facilities, etc,

        // Dietary and Health Information
        public string? dietary_restrictions { get; set; } // e.g. Vegetarian, Vegan, Gluten-Free, Religious, etc, etc
        public string? food_allergy { get; set; } // e.g. Peanuts, Shellfish, etc,
        public string? medical_condition { get; set; } // e.g. Diabetes, Heart Condition, etc,
        public string? special_needs { get; set; } // e.g. Wheelchair Access, etc,
        public string? wellness_preference { get; set; } // e.g. Spa, Gym, etc,

        [ForeignKey("user_id")]
        public User User { get; set; }  // Navigation property
        public List<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
