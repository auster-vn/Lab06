namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class SupplyTrashItemViewModel
    {
        public int Id { get; set; }
        public string SupplyCode { get; set; } = string.Empty;
        public string SupplyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
    }
}
