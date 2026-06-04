namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class RequestHistoryViewModel
    {
        public int Id { get; set; }
        public string RequestCode { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<RequestIssueItemViewModel> Items { get; set; } = new();
    }

    public class RequestIssueItemViewModel
    {
        public string SupplyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
