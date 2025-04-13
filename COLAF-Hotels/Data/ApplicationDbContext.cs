using Microsoft.EntityFrameworkCore;
using COLAFHotel.Models;
using COLAF_Hotels.Models;

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
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Invoice> Invoices { get; set; } 
        public DbSet<Service> Services { get; set; } 
        public DbSet<Booking_Service> Booking_Services { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure the table names match exactly with your existing PostgreSQL schema
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Room>().ToTable("room");
            modelBuilder.Entity<Guest>().ToTable("guest");
            modelBuilder.Entity<Booking>().ToTable("booking");
            modelBuilder.Entity<Discount>().ToTable("discount");
            modelBuilder.Entity<Invoice>().ToTable("invoice");
            modelBuilder.Entity<Service>().ToTable("special_service");
            modelBuilder.Entity<HousekeepingTask>().ToTable("housekeeping_task");
            modelBuilder.Entity<Payment>().ToTable("payment");
            modelBuilder.Entity<Booking_Service>().ToTable("booking_service");
          

            //Booking-Service relationship
            modelBuilder.Entity<Booking_Service>()
                .HasKey(bs => new { bs.booking_id, bs.service_id });

            modelBuilder.Entity<Booking_Service>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.booking_id);

            modelBuilder.Entity<Booking_Service>()
                .HasOne(bs => bs.Service)
                .WithMany(s => s.BookingServices)
                .HasForeignKey(bs => bs.service_id);
        }
    }
}
