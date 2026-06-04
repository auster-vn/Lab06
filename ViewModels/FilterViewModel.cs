namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class FilterViewModel
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<SupplyListItemViewModel> Results { get; set; } = new();
    }
}
