using System;

namespace MedicalSuppliesCatalog.Lab06.Models
{
    /// <summary>
    /// Audit log ghi lại các hành vi nhạy cảm để kiểm toán bảo mật.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string Result { get; set; } = "Success"; // "Success" or "Failed"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        public string? Hash { get; set; } // Cryptographic hash for tamper detection
    }
}
