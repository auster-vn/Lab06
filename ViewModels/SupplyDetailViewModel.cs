namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class SupplyDetailViewModel
    {
        public int Id { get; set; }
        public string SupplyCode { get; set; } = string.Empty;
        public string SupplyName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsQuarantined { get; set; }
        public string? QuarantineReason { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal InventoryValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
