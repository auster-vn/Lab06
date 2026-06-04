namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class DataHealthViewModel
    {
        public bool DatabaseConnected { get; set; }
        public int MigrationCount { get; set; }
        public int SupplyCategoryCount { get; set; }
        public int MedicalSupplyCount { get; set; }
        public int RequestCount { get; set; }
        public string NoTrackingDemoResult { get; set; } = string.Empty;
        public bool TransactionSupported { get; set; }
        public string AppName { get; set; } = string.Empty;
        public int LowStockThreshold { get; set; }
        public List<string> LowStockSupplies { get; set; } = new();
    }
}
