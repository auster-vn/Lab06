using System.ComponentModel.DataAnnotations;

namespace MedicalSuppliesCatalog.Lab06.Models
{
    /// <summary>
    /// Entity vật tư y tế tích hợp đầy đủ mối quan hệ, Concurrency token, Audit fields, Soft Delete và Ảnh.
    /// </summary>
    public class MedicalSupply
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string SupplyName { get; set; } = string.Empty;

        [Range(0, 999999999)]
        public decimal UnitPrice { get; set; }

        [Range(0, 1000000)]
        public int StockQuantity { get; set; }

        [Range(0, 100000)]
        public int MinimumStock { get; set; }

        [MaxLength(200)]
        public string Supplier { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        // ===== Creative Feature: Quality Alert & Quarantine Status =====
        public bool IsQuarantined { get; set; }
        [MaxLength(200)]
        public string? QuarantineReason { get; set; }

        // ===== Relationship =====
        [Required]
        public int CategoryId { get; set; }
        public SupplyCategory? Category { get; set; }

        public ICollection<SupplyIssue> SupplyIssues { get; set; } = new List<SupplyIssue>();

        // ===== Audit Fields =====
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===== Soft Delete Fields =====
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // ===== Concurrency Token =====
        public byte[]? RowVersion { get; set; }
    }
}
