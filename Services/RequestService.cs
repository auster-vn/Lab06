using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;
using MedicalSuppliesCatalog.Lab06.Repositories;
using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRequestRepository _requestRepository;
        private readonly IAuditLogService _auditLogService;

        public RequestService(
            ApplicationDbContext context, 
            IRequestRepository requestRepository, 
            IAuditLogService auditLogService)
        {
            _context = context;
            _requestRepository = requestRepository;
            _auditLogService = auditLogService;
        }

        public async Task CreateRequestAsync(RequestCreateViewModel model)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var supply = await _context.MedicalSupplies
                    .FirstOrDefaultAsync(s => s.Id == model.MedicalSupplyId);

                if (supply == null)
                {
                    await _auditLogService.LogAsync("CreateRequest", "SupplyRequest", null, "Failed", "Vật tư không tồn tại.");
                    throw new Exception("Medical supply not found.");
                }

                // Creative Feature: Quarantine validation
                if (supply.IsQuarantined)
                {
                    await _auditLogService.LogAsync("CreateRequest", "SupplyRequest", null, "Failed", 
                        $"Từ chối cấp phát {supply.SupplyName} do đang bị CÁCH LY ({supply.QuarantineReason}).");
                    throw new Exception($"Vật tư này đang bị CÁCH LY kiểm chuẩn ({supply.QuarantineReason}), không thể cấp phát!");
                }

                if (supply.StockQuantity < model.Quantity)
                {
                    await _auditLogService.LogAsync("CreateRequest", "SupplyRequest", null, "Failed", 
                        $"Không đủ tồn kho cho {supply.SupplyName}. Yêu cầu: {model.Quantity}, Hiện có: {supply.StockQuantity}.");
                    throw new Exception($"Not enough stock. Available: {supply.StockQuantity}.");
                }

                // Create request
                var request = new SupplyRequest
                {
                    RequestCode = $"REQ-{DateTime.Now:yyyyMMddHHmmss}",
                    RequesterName = model.RequesterName,
                    Department = model.Department,
                    CreatedAt = DateTime.Now,
                    TotalAmount = supply.UnitPrice * model.Quantity,
                    Status = "Completed"
                };
                _context.SupplyRequests.Add(request);
                await _context.SaveChangesAsync();

                // Create issue item
                var issue = new SupplyIssue
                {
                    SupplyRequestId = request.Id,
                    MedicalSupplyId = supply.Id,
                    Quantity = model.Quantity,
                    UnitPrice = supply.UnitPrice
                };
                _context.SupplyIssues.Add(issue);

                // Deduct stock
                supply.StockQuantity -= model.Quantity;
                supply.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log audit trail
                await _auditLogService.LogAsync("CreateRequest", "SupplyRequest", request.Id.ToString(), "Success", 
                    $"Cấp phát {model.Quantity}x {supply.SupplyName} cho {model.RequesterName} ({model.Department}).");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<RequestHistoryViewModel>> GetHistoryAsync()
        {
            var requests = await _requestRepository.GetAllWithIssuesAsync();

            return requests.Select(r => new RequestHistoryViewModel
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                RequesterName = r.RequesterName,
                Department = r.Department,
                CreatedAt = r.CreatedAt,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                Items = r.SupplyIssues.Select(i => new RequestIssueItemViewModel
                {
                    SupplyName = i.MedicalSupply?.SupplyName ?? "N/A",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList();
        }
    }
}
