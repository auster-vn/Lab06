namespace MedicalSuppliesCatalog.Lab06.Models
{
    /// <summary>
    /// Chi tiết cấp phát vật tư (tương đương OrderItem)
    /// </summary>
    public class SupplyIssue
    {
        public int Id { get; set; }

        public int SupplyRequestId { get; set; }
        public SupplyRequest? SupplyRequest { get; set; }

        public int MedicalSupplyId { get; set; }
        public MedicalSupply? MedicalSupply { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
