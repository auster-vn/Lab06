namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalSupplies { get; set; }
        public int ActiveSupplies { get; set; }
        public int DeletedSupplies { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TodayLogsCount { get; set; }

        // Security Dashboard stats (Feature 3)
        public int AccessDeniedToday { get; set; }
        public int SensitiveOpsToday { get; set; }
        public int RejectedUploadsToday { get; set; }
    }
}
