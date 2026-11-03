
using Microsoft.EntityFrameworkCore;
using RentEase.Models;

namespace RentEase.Data
{
    public class RentEaseDbContext : DbContext
    {
        public RentEaseDbContext(DbContextOptions<RentEaseDbContext> options)
        : base(options)
        {
        }
        // Database tables
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<RoomAllocation> RoomAllocations { get; set; }
        public DbSet<RentalAgreement> RentalAgreements { get; set; }
        public DbSet<RentPayment> RentPayments { get; set; }
        public DbSet<UtilityBill> UtilityBills { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceExpense> MaintenanceExpenses { get; set; }
        public DbSet<TenantDocument> TenantDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser - Property (One landlord has many properties)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Landlord)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser - Tenant (One landlord creates many tenants)
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Landlord)
                .WithMany(u => u.CreatedTenants)
                .HasForeignKey(t => t.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser - Tenant (One tenant has one login account)
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<Tenant>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property - Room (One property has many rooms)
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Property)
                .WithMany(p => p.Rooms)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Room - RoomAllocation
            modelBuilder.Entity<RoomAllocation>()
                .HasOne(a => a.Room)
                .WithMany(r => r.RoomAllocations)
                .HasForeignKey(a => a.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tenant - RoomAllocation
            modelBuilder.Entity<RoomAllocation>()
                .HasOne(a => a.Tenant)
                .WithMany(t => t.RoomAllocations)
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // RoomAllocation - RentalAgreement
            modelBuilder.Entity<RentalAgreement>()
                .HasOne(a => a.RoomAllocation)
                .WithMany(r => r.RentalAgreements)
                .HasForeignKey(a => a.AllocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // RentalAgreement - RentPayment
            modelBuilder.Entity<RentPayment>()
                .HasOne(p => p.RentalAgreement)
                .WithMany(a => a.RentPayments)
                .HasForeignKey(p => p.AgreementId)
                .OnDelete(DeleteBehavior.Restrict);

            // RentalAgreement - UtilityBill
            modelBuilder.Entity<UtilityBill>()
                .HasOne(b => b.RentalAgreement)
                .WithMany(a => a.UtilityBills)
                .HasForeignKey(b => b.AgreementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tenant - MaintenanceRequest
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Tenant)
                .WithMany()
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property - MaintenanceExpense
            modelBuilder.Entity<MaintenanceExpense>()
                .HasOne(e => e.Property)
                .WithMany()
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tenant - TenantDocument
            modelBuilder.Entity<TenantDocument>()
                .HasOne(d => d.Tenant)
                .WithMany()
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique email for every account
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Room number should be unique within a property
            modelBuilder.Entity<Room>()
                .HasIndex(r => new { r.PropertyId, r.RoomNumber })
                .IsUnique();

            // Decimal precision
            modelBuilder.Entity<Room>()
                .Property(r => r.MonthlyRent)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RentalAgreement>()
                .Property(a => a.MonthlyRent)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RentalAgreement>()
                .Property(a => a.SecurityDeposit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RentPayment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UtilityBill>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceExpense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);
        }
    }
}
