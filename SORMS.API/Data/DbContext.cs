namespace SORMS.API.Data
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Models;

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
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CheckInRecord> CheckInRecords { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Resident - Room =====
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.Room)
                .WithMany(rm => rm.Residents)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict); // 🔑 Tránh cascade vòng lặp

            // ===== ServiceRequest - Resident =====
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Resident)
                .WithMany(r => r.ServiceRequests)
                .HasForeignKey(sr => sr.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== ServiceRequest - Staff =====
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Staff)
                .WithMany(s => s.AssignedRequests)
                .HasForeignKey(sr => sr.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Billing - Resident =====
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Resident)
                .WithMany(r => r.Billings)
                .HasForeignKey(b => b.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Notification - Resident =====
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Resident)
                .WithMany()
                .HasForeignKey(n => n.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== User - Role =====
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CheckInRecord - Resident =====
            modelBuilder.Entity<CheckInRecord>()
                .HasOne(c => c.Resident)
                .WithMany()
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CheckInRecord - Room =====
            modelBuilder.Entity<CheckInRecord>()
                .HasOne(c => c.Room)
                .WithMany()
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
