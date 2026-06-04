namespace MedicalSuppliesCatalog.Lab06.Models
{
    /// <summary>
    /// Yêu cầu cấp phát vật tư (tương đương Order trong Mini Shop)
    /// </summary>
    public class SupplyRequest
    {
        public int Id { get; set; }

        public string RequestCode { get; set; } = string.Empty;

        public string RequesterName { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        // Navigation: One request → Many issue items
        public ICollection<SupplyIssue> SupplyIssues { get; set; } = new List<SupplyIssue>();
    }
}
