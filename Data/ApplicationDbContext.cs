using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SupplyCategory> SupplyCategories => Set<SupplyCategory>();
        public DbSet<MedicalSupply> MedicalSupplies => Set<MedicalSupply>();
        public DbSet<SupplyRequest> SupplyRequests => Set<SupplyRequest>();
        public DbSet<SupplyIssue> SupplyIssues => Set<SupplyIssue>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SupplyCategory mapping
            builder.Entity<SupplyCategory>(entity =>
            {
                entity.ToTable("SupplyCategories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            // MedicalSupply mapping
            builder.Entity<MedicalSupply>(entity =>
            {
                entity.ToTable("MedicalSupplies");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.SupplyCode).IsRequired().HasMaxLength(50);
                entity.Property(s => s.SupplyName).IsRequired().HasMaxLength(200);
                entity.Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Supplier).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).HasMaxLength(1000);
                entity.Property(s => s.ImagePath).HasMaxLength(500);

                // Unique index on SupplyCode
                entity.HasIndex(s => s.SupplyCode).IsUnique();

                // Relationship: Many supplies -> One category
                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Supplies)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // RowVersion for SQLite compatibility (as optional concurrency token)
                entity.Property(s => s.RowVersion)
                      .IsRowVersion()
                      .IsConcurrencyToken();

                // Global Query Filter: auto-filter out soft-deleted items
                entity.HasQueryFilter(s => !s.IsDeleted);
            });

            // SupplyRequest mapping
            builder.Entity<SupplyRequest>(entity =>
            {
                entity.ToTable("SupplyRequests");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RequestCode).IsRequired().HasMaxLength(50);
                entity.Property(r => r.RequesterName).IsRequired().HasMaxLength(150);
                entity.Property(r => r.Department).IsRequired().HasMaxLength(100);
                entity.Property(r => r.TotalAmount).HasColumnType("decimal(18,2)");
            });

            // SupplyIssue mapping
            builder.Entity<SupplyIssue>(entity =>
            {
                entity.ToTable("SupplyIssues");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(i => i.SupplyRequest)
                      .WithMany(r => r.SupplyIssues)
                      .HasForeignKey(i => i.SupplyRequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.MedicalSupply)
                      .WithMany(s => s.SupplyIssues)
                      .HasForeignKey(i => i.MedicalSupplyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog mapping
            builder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Action).IsRequired().HasMaxLength(100);
                entity.Property(l => l.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(l => l.EntityId).HasMaxLength(100);
                entity.Property(l => l.UserName).HasMaxLength(150);
                entity.Property(l => l.IpAddress).HasMaxLength(50);
                entity.Property(l => l.Result).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Note).HasMaxLength(500);
            });

            // ===== SEED DATA =====
            builder.Entity<SupplyCategory>().HasData(
                new SupplyCategory { Id = 1, Name = "Protective Equipment", Description = "PPE and protective gear" },
                new SupplyCategory { Id = 2, Name = "Diagnostic", Description = "Diagnostic instruments" },
                new SupplyCategory { Id = 3, Name = "Injection Supplies", Description = "Syringes, needles and related" },
                new SupplyCategory { Id = 4, Name = "Sanitation", Description = "Cleaning and sanitation products" },
                new SupplyCategory { Id = 5, Name = "First Aid", Description = "First aid kits and supplies" }
            );

            builder.Entity<MedicalSupply>().HasData(
                new MedicalSupply
                {
                    Id = 1, SupplyCode = "MED-001", SupplyName = "Surgical Mask",
                    CategoryId = 1, Supplier = "MediCare Co.", UnitPrice = 5000,
                    StockQuantity = 200, MinimumStock = 50, Description = "3-layer surgical mask, disposable",
                    CreatedAt = new DateTime(2026, 5, 1), IsDeleted = false, IsQuarantined = false
                },
                new MedicalSupply
                {
                    Id = 2, SupplyCode = "MED-002", SupplyName = "Disposable Gloves",
                    CategoryId = 1, Supplier = "HealthPlus", UnitPrice = 2000,
                    StockQuantity = 30, MinimumStock = 50, Description = "Latex-free nitrile gloves",
                    CreatedAt = new DateTime(2026, 5, 2), IsDeleted = false, IsQuarantined = false
                },
                new MedicalSupply
                {
                    Id = 3, SupplyCode = "MED-003", SupplyName = "Digital Thermometer",
                    CategoryId = 2, Supplier = "BioTech", UnitPrice = 150000,
                    StockQuantity = 0, MinimumStock = 10, Description = "Infrared non-contact thermometer",
                    CreatedAt = new DateTime(2026, 4, 28), IsDeleted = false, IsQuarantined = false
                },
                new MedicalSupply
                {
                    Id = 4, SupplyCode = "MED-004", SupplyName = "Syringe 5ml",
                    CategoryId = 3, Supplier = "SafeMed", UnitPrice = 3500,
                    StockQuantity = 120, MinimumStock = 40, Description = "Sterile disposable syringe",
                    CreatedAt = new DateTime(2026, 5, 3), IsDeleted = false, IsQuarantined = false
                },
                new MedicalSupply
                {
                    Id = 5, SupplyCode = "MED-005", SupplyName = "Blood Pressure Monitor",
                    CategoryId = 2, Supplier = "MediTech", UnitPrice = 850000,
                    StockQuantity = 8, MinimumStock = 10, Description = "Digital automatic BP monitor",
                    CreatedAt = new DateTime(2026, 4, 25), IsDeleted = false, IsQuarantined = false
                }
            );
        }
    }
}
