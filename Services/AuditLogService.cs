using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            ApplicationDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<AuditLogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityName, string? entityId, string result, string? note = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userName = httpContext?.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            var log = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                UserName = userName,
                IpAddress = ipAddress,
                Result = result,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                // Structured logging for operations
                _logger.LogInformation("Audit Logged: Action={Action}, Entity={EntityName}, Id={EntityId}, User={User}, IP={IP}, Result={Result}, Note={Note}",
                    action, entityName, entityId, userName, ipAddress, result, note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log to database.");
            }
        }

        public async Task<List<AuditLog>> SearchLogsAsync(string? userName, string? action, string? result, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(l => l.UserName != null && l.UserName.ToLower().Contains(userName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(l => l.Action.ToLower().Contains(action.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                query = query.Where(l => l.Result.ToLower() == result.ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= endDate.Value);
            }

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}
