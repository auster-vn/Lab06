namespace MedicalSuppliesCatalog.Lab06.Models
{
    /// <summary>
    /// Danh mục vật tư y tế (One-to-Many với MedicalSupply)
    /// </summary>
    public class SupplyCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation property: One category → Many supplies
        public ICollection<MedicalSupply> Supplies { get; set; } = new List<MedicalSupply>();
    }
}
