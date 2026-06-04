using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalSuppliesCatalog.Lab06.Services;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    [Authorize(Policy = "CanViewAuditLog")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: /AuditLogs
        // GET: /AuditLogs?userName=...&action=...&result=...&startDate=...&endDate=...
        public async Task<IActionResult> Index(
            string? userName, 
            string? logAction, 
            string? result, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            var logs = await _auditLogService.SearchLogsAsync(userName, logAction, result, startDate, endDate);
            
            // Perform integrity check for all displayed logs
            var verification = new Dictionary<int, bool>();
            foreach (var log in logs)
            {
                verification[log.Id] = _auditLogService.VerifyLog(log);
            }
            ViewBag.VerificationResults = verification;
            
            // Pass parameters back to View for persistent form values
            ViewData["userName"] = userName;
            ViewData["logAction"] = logAction;
            ViewData["result"] = result;
            ViewData["startDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["endDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(logs);
        }
    }
}
