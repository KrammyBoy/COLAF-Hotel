using Microsoft.EntityFrameworkCore;
using COLAFHotel.Models;

namespace COLAFHotel.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        // Define the tables that exist in your PostgreSQL database
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure the table names match exactly with your existing PostgreSQL schema
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Room>().ToTable("room");
            modelBuilder.Entity<Guest>().ToTable("guest");
            modelBuilder.Entity<Booking>().ToTable("booking");
        }
    }
}
