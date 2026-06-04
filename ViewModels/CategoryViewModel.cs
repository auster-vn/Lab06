namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SupplyCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}
