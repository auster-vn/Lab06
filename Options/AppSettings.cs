namespace MedicalSuppliesCatalog.Lab06.Options
{
    /// <summary>
    /// Strongly-typed configuration object — Options Pattern
    /// Bind từ section "AppSettings" trong appsettings.json
    /// </summary>
    public class AppSettings
    {
        public string AppName { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public bool EnableSeedData { get; set; }

        /// <summary>
        /// Feature 1: Ngưỡng tồn kho cảnh báo (LowStockThreshold)
        /// Thay đổi giá trị trong appsettings.json → kết quả hiển thị thay đổi,
        /// không cần sửa code nghiệp vụ.
        /// </summary>
        public int LowStockThreshold { get; set; } = 20;
    }
}
