using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityName, string? entityId, string result, string? note = null);
        Task<List<AuditLog>> SearchLogsAsync(string? userName, string? action, string? result, DateTime? startDate, DateTime? endDate);
        bool VerifyLog(AuditLog log);
    }
}
