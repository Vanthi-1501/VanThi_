using Microsoft.EntityFrameworkCore;
using VanThi.Models;

namespace VanThi.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Stay> Stays { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Precision cho các trường decimal
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // Xử lý Multiple Cascade Paths
            modelBuilder.Entity<Stay>()
                .HasOne(s => s.Room)
                .WithMany()
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceOrder>()
                .HasOne(so => so.Stay)
                .WithMany()
                .HasForeignKey(so => so.StayId)
                .OnDelete(DeleteBehavior.NoAction);

            // Seed Dịch vụ ăn uống
            modelBuilder.Entity<Service>().HasData(
                new Service { Id = 1, Name = "Ăn sáng", Price = 10 },
                new Service { Id = 2, Name = "Ăn trưa", Price = 20 },
                new Service { Id = 3, Name = "Ăn tối", Price = 30 }
            );
        }
    }
}