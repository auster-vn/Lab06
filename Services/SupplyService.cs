using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;
using MedicalSuppliesCatalog.Lab06.Repositories;
using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public class SupplyService : ISupplyService
    {
        private readonly ISupplyRepository _supplyRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplyService> _logger;

        public SupplyService(
            ISupplyRepository supplyRepository, 
            IAuditLogService auditLogService,
            ApplicationDbContext context,
            ILogger<SupplyService> logger)
        {
            _supplyRepository = supplyRepository;
            _auditLogService = auditLogService;
            _context = context;
            _logger = logger;
        }

        public async Task<List<SupplyListItemViewModel>> GetActiveSuppliesAsync()
        {
            var supplies = await _supplyRepository.GetAllReadOnlyAsync();
            return MapToListItemViewModels(supplies);
        }

        public async Task<List<SupplyListItemViewModel>> SearchSuppliesAsync(string? keyword)
        {
            var supplies = await _supplyRepository.SearchAsync(keyword);
            return MapToListItemViewModels(supplies);
        }

        public async Task<SupplyDetailViewModel?> GetDetailAsync(int id)
        {
            var s = await _supplyRepository.GetByIdAsync(id);
            if (s == null) return null;

            return new SupplyDetailViewModel
            {
                Id = s.Id,
                SupplyCode = s.SupplyCode,
                SupplyName = s.SupplyName,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Name ?? "N/A",
                Supplier = s.Supplier,
                UnitPrice = s.UnitPrice,
                StockQuantity = s.StockQuantity,
                MinimumStock = s.MinimumStock,
                Description = s.Description,
                ImagePath = s.ImagePath,
                IsQuarantined = s.IsQuarantined,
                QuarantineReason = s.QuarantineReason,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                InventoryValue = s.UnitPrice * s.StockQuantity,
                Status = s.IsQuarantined ? "Quarantined"
                       : s.StockQuantity == 0 ? "Out of Stock"
                       : s.StockQuantity <= s.MinimumStock ? "Low Stock"
                       : "Available"
            };
        }

        public async Task<SupplyEditViewModel?> GetForEditAsync(int id)
        {
            var s = await _supplyRepository.GetByIdAsync(id);
            if (s == null) return null;

            return new SupplyEditViewModel
            {
                Id = s.Id,
                SupplyCode = s.SupplyCode,
                SupplyName = s.SupplyName,
                CategoryId = s.CategoryId,
                Supplier = s.Supplier,
                UnitPrice = s.UnitPrice,
                StockQuantity = s.StockQuantity,
                MinimumStock = s.MinimumStock,
                Description = s.Description,
                ImagePath = s.ImagePath,
                IsQuarantined = s.IsQuarantined,
                QuarantineReason = s.QuarantineReason,
                RowVersionBase64 = s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : string.Empty
            };
        }

        public async Task CreateAsync(SupplyCreateViewModel model)
        {
            var supply = new MedicalSupply
            {
                SupplyCode = model.SupplyCode.ToUpper().Trim(),
                SupplyName = model.SupplyName.Trim(),
                CategoryId = model.CategoryId,
                Supplier = model.Supplier.Trim(),
                UnitPrice = model.UnitPrice,
                StockQuantity = model.StockQuantity,
                MinimumStock = model.MinimumStock,
                Description = model.Description?.Trim(),
                IsQuarantined = model.IsQuarantined,
                QuarantineReason = model.IsQuarantined ? model.QuarantineReason?.Trim() : null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _supplyRepository.AddAsync(supply);
            await _supplyRepository.SaveChangesAsync();

            await _auditLogService.LogAsync("CreateSupply", "MedicalSupply", supply.Id.ToString(), "Success", 
                $"Tạo mới vật tư: {supply.SupplyCode} - {supply.SupplyName}");
        }

        public async Task<bool> EditAsync(SupplyEditViewModel model)
        {
            var supply = await _supplyRepository.GetByIdWithTrackingAsync(model.Id);
            if (supply == null)
            {
                await _auditLogService.LogAsync("EditSupply", "MedicalSupply", model.Id.ToString(), "Failed", "Vật tư không tồn tại.");
                return false;
            }

            // Concurrency control via OriginalValue mapping
            if (!string.IsNullOrEmpty(model.RowVersionBase64))
            {
                _context.Entry(supply).Property(s => s.RowVersion).OriginalValue = Convert.FromBase64String(model.RowVersionBase64);
            }

            supply.SupplyCode = model.SupplyCode.ToUpper().Trim();
            supply.SupplyName = model.SupplyName.Trim();
            supply.CategoryId = model.CategoryId;
            supply.Supplier = model.Supplier.Trim();
            supply.UnitPrice = model.UnitPrice;
            supply.StockQuantity = model.StockQuantity;
            supply.MinimumStock = model.MinimumStock;
            supply.Description = model.Description?.Trim();
            supply.IsQuarantined = model.IsQuarantined;
            supply.QuarantineReason = model.IsQuarantined ? model.QuarantineReason?.Trim() : null;
            supply.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _supplyRepository.SaveChangesAsync();
                await _auditLogService.LogAsync("EditSupply", "MedicalSupply", supply.Id.ToString(), "Success", 
                    $"Cập nhật vật tư: {supply.SupplyCode}");
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during editing Supply Id={Id}", model.Id);
                await _auditLogService.LogAsync("EditSupply", "MedicalSupply", model.Id.ToString(), "Failed", 
                    "Xung đột dữ liệu (Concurrency Conflict) - Dữ liệu đã thay đổi bởi người khác.");
                throw;
            }
        }

        public async Task<bool> AdjustStockAsync(int id, int stockQuantityChange, string rowVersionStr)
        {
            var supply = await _supplyRepository.GetByIdWithTrackingAsync(id);
            if (supply == null)
            {
                await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", id.ToString(), "Failed", "Vật tư không tồn tại.");
                return false;
            }

            if (!string.IsNullOrEmpty(rowVersionStr))
            {
                _context.Entry(supply).Property(s => s.RowVersion).OriginalValue = Convert.FromBase64String(rowVersionStr);
            }

            int originalStock = supply.StockQuantity;
            int newStock = originalStock + stockQuantityChange;

            if (newStock < 0)
            {
                await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", id.ToString(), "Failed", 
                    $"Điều chỉnh tồn kho không hợp lệ. Hiện tại: {originalStock}, Yêu cầu thay đổi: {stockQuantityChange}. Tồn kho không thể âm.");
                throw new ArgumentException("Tồn kho sau điều chỉnh không thể nhỏ hơn 0.");
            }

            supply.StockQuantity = newStock;
            supply.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _supplyRepository.SaveChangesAsync();
                await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", supply.Id.ToString(), "Success", 
                    $"Điều chỉnh tồn kho cho {supply.SupplyCode}. Thay đổi: {stockQuantityChange} (Từ {originalStock} thành {newStock}).");
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during adjusting stock for Supply Id={Id}", id);
                await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", id.ToString(), "Failed", 
                    "Xung đột dữ liệu khi điều chỉnh tồn kho. Vui lòng tải lại trang.");
                throw;
            }
        }

        public async Task<bool> ToggleQuarantineAsync(int id, bool isQuarantined, string? reason)
        {
            var supply = await _supplyRepository.GetByIdWithTrackingAsync(id);
            if (supply == null) return false;

            supply.IsQuarantined = isQuarantined;
            supply.QuarantineReason = isQuarantined ? reason?.Trim() : null;
            supply.UpdatedAt = DateTime.UtcNow;

            await _supplyRepository.SaveChangesAsync();

            string statusText = isQuarantined ? $"CÁCH LY (Lý do: {reason})" : "GIẢI PHÓNG CÁCH LY";
            await _auditLogService.LogAsync("ToggleQuarantine", "MedicalSupply", supply.Id.ToString(), "Success", 
                $"Thay đổi trạng thái vật tư {supply.SupplyCode} thành {statusText}");

            return true;
        }

        public async Task<bool> UpdateImagePathAsync(int id, string? imagePath)
        {
            var supply = await _supplyRepository.GetByIdWithTrackingAsync(id);
            if (supply == null) return false;

            supply.ImagePath = imagePath;
            supply.UpdatedAt = DateTime.UtcNow;

            await _supplyRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var supply = await _supplyRepository.GetByIdWithTrackingAsync(id);
            if (supply == null) return false;

            supply.IsDeleted = true;
            supply.DeletedAt = DateTime.UtcNow;
            supply.UpdatedAt = DateTime.UtcNow;

            await _supplyRepository.SaveChangesAsync();

            await _auditLogService.LogAsync("SoftDeleteSupply", "MedicalSupply", supply.Id.ToString(), "Success", 
                $"Xóa mềm vật tư: {supply.SupplyCode}");
            return true;
        }

        public async Task<List<SupplyTrashItemViewModel>> GetTrashAsync()
        {
            var trashList = await _supplyRepository.GetTrashReadOnlyAsync();
            return trashList.Select(s => new SupplyTrashItemViewModel
            {
                Id = s.Id,
                SupplyCode = s.SupplyCode,
                SupplyName = s.SupplyName,
                CategoryName = s.Category?.Name ?? "N/A",
                DeletedAt = s.DeletedAt
            }).ToList();
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var supply = await _supplyRepository.GetByIdFromTrashAsync(id);
            if (supply == null) return false;

            supply.IsDeleted = false;
            supply.DeletedAt = null;
            supply.UpdatedAt = DateTime.UtcNow;

            await _supplyRepository.SaveChangesAsync();

            await _auditLogService.LogAsync("RestoreSupply", "MedicalSupply", supply.Id.ToString(), "Success", 
                $"Khôi phục vật tư: {supply.SupplyCode}");
            return true;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            // IgnoreQueryFilters to count deleted supplies too
            var allSupplies = await _context.MedicalSupplies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            var active = allSupplies.Where(s => !s.IsDeleted).ToList();

            var today = DateTime.Today;
            var todayLogsCount = await _context.AuditLogs
                .AsNoTracking()
                .CountAsync(l => l.CreatedAt.Date == today);

            // Dashboard stats for security audits
            var accessDeniedCount = await _context.AuditLogs
                .AsNoTracking()
                .CountAsync(l => l.Action == "AccessDenied" && l.CreatedAt.Date == today);

            var sensitiveOpsCount = await _context.AuditLogs
                .AsNoTracking()
                .CountAsync(l => (l.Action == "CreateSupply" || l.Action == "EditSupply" || l.Action == "SoftDeleteSupply" || l.Action == "RestoreSupply" || l.Action == "AdjustStock" || l.Action == "ToggleQuarantine" || l.Action == "ReplaceImage") && l.CreatedAt.Date == today);

            var rejectedUploadsCount = await _context.AuditLogs
                .AsNoTracking()
                .CountAsync(l => l.Action == "UploadImage" && l.Result == "Failed" && l.CreatedAt.Date == today);

            return new DashboardViewModel
            {
                TotalSupplies = allSupplies.Count,
                ActiveSupplies = active.Count,
                DeletedSupplies = allSupplies.Count(s => s.IsDeleted),
                OutOfStockCount = active.Count(s => s.StockQuantity == 0),
                LowStockCount = active.Count(s => s.StockQuantity > 0 && s.StockQuantity <= s.MinimumStock),
                TotalInventoryValue = active.Sum(s => s.UnitPrice * s.StockQuantity),
                TodayLogsCount = todayLogsCount,
                AccessDeniedToday = accessDeniedCount,
                SensitiveOpsToday = sensitiveOpsCount,
                RejectedUploadsToday = rejectedUploadsCount
            };
        }

        private List<SupplyListItemViewModel> MapToListItemViewModels(List<MedicalSupply> supplies)
        {
            return supplies.Select(s => new SupplyListItemViewModel
            {
                Id = s.Id,
                SupplyCode = s.SupplyCode,
                SupplyName = s.SupplyName,
                CategoryName = s.Category?.Name ?? "N/A",
                Supplier = s.Supplier,
                UnitPrice = s.UnitPrice,
                StockQuantity = s.StockQuantity,
                MinimumStock = s.MinimumStock,
                ImagePath = s.ImagePath,
                IsQuarantined = s.IsQuarantined,
                CreatedAt = s.CreatedAt,
                Status = s.IsQuarantined ? "Quarantined"
                       : s.StockQuantity == 0 ? "Out of Stock"
                       : s.StockQuantity <= s.MinimumStock ? "Low Stock"
                       : "Available"
            }).ToList();
        }
    }
}
