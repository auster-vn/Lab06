namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class SupplyListItemViewModel
    {
        public int Id { get; set; }
        public string SupplyCode { get; set; } = string.Empty;
        public string SupplyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsQuarantined { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
