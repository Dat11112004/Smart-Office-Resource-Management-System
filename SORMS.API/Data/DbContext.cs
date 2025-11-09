using Microsoft.EntityFrameworkCore;
using SORMS.API.Models;

namespace SORMS.API.Data
{
    public class SormsDbContext : DbContext
    {
        public SormsDbContext(DbContextOptions<SormsDbContext> options)
            : base(options)
        {
        }

        // ===== DbSet cho các bảng =====
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CheckInRecord> CheckInRecords { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================
            // 🔹 Resident ↔ Room (1-n)
            // ==========================
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.Room)
                .WithMany(rm => rm.Residents)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh cascade vòng lặp

            // ==========================
            // 🔹 Resident ↔ Billing (1-n)
            // ==========================
            
            // ==========================
            // 🔹 Resident ↔ Notification (1-n)
            // ==========================
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Resident)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🔹 Resident ↔ CheckInRecord (1-n)
            // ==========================
            modelBuilder.Entity<CheckInRecord>()
                .HasOne(c => c.Resident)
                .WithMany(r => r.CheckInRecords)
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🔹 Room ↔ CheckInRecord (1-n)
            // ==========================
            modelBuilder.Entity<CheckInRecord>()
                .HasOne(c => c.Room)
                .WithMany(rm => rm.CheckInRecords)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🔹 ServiceRequest ↔ Resident (1-n)
            // ==========================
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Resident)
                .WithMany(r => r.ServiceRequests)
                .HasForeignKey(sr => sr.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🔹 User ↔ Role (1-n)
            // ==========================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🔹 Optional: Seed dữ liệu mẫu
            // ==========================
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "System administrator" },
                new Role { Id = 2, Name = "Staff", Description = "Staff member" },
                new Role { Id = 3, Name = "Resident", Description = "Dormitory resident" }
            );

           
            
        }
    }
}
