using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Services;
using MedicalSuppliesCatalog.Lab06.ViewModels;
using System.Security.Claims;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    [Authorize(Policy = "CanViewProduct")]
    public class SuppliesController : Controller
    {
        private readonly ISupplyService _supplyService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IAuditLogService _auditLogService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SuppliesController> _logger;

        public SuppliesController(
            ISupplyService supplyService, 
            IFileUploadService fileUploadService,
            IAuditLogService auditLogService,
            ApplicationDbContext context,
            ILogger<SuppliesController> logger)
        {
            _supplyService = supplyService;
            _fileUploadService = fileUploadService;
            _auditLogService = auditLogService;
            _context = context;
            _logger = logger;
        }

        // GET: /Supplies
        public async Task<IActionResult> Index()
        {
            var supplies = await _supplyService.GetActiveSuppliesAsync();
            return View(supplies);
        }

        // GET: /Supplies/Search?keyword=...
        public async Task<IActionResult> Search(string? keyword)
        {
            var supplies = await _supplyService.SearchSuppliesAsync(keyword);
            ViewData["keyword"] = keyword;
            return View("Index", supplies);
        }

        // GET: /Supplies/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var supply = await _supplyService.GetDetailAsync(id);
            if (supply == null)
            {
                _logger.LogWarning("Detail not found. Id={Id}", id);
                return NotFound();
            }
            return View(supply);
        }

        // GET: /Supplies/Create
        [HttpGet]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdownAsync();
            return View(new SupplyCreateViewModel());
        }

        // POST: /Supplies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Create(SupplyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync();
                return View(model);
            }

            try
            {
                await _supplyService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Đã tạo vật tư '{model.SupplyName}' thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi lưu dữ liệu: {ex.Message}");
                await PopulateCategoriesDropdownAsync();
                return View(model);
            }
        }

        // GET: /Supplies/Edit/{id}
        [HttpGet]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _supplyService.GetForEditAsync(id);
            if (vm == null) return NotFound();

            await PopulateCategoriesDropdownAsync();
            return View(vm);
        }

        // POST: /Supplies/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Edit(int id, SupplyEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync();
                return View(model);
            }

            try
            {
                var result = await _supplyService.EditAsync(model);
                if (!result) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật vật tư thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty,
                    "Dữ liệu đã bị thay đổi bởi người dùng khác kể từ khi bạn mở trang. Vui lòng tải lại và thử lại.");
                _logger.LogWarning("Concurrency conflict on Edit. SupplyId={Id}", id);
                await PopulateCategoriesDropdownAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi lưu dữ liệu: {ex.Message}");
                await PopulateCategoriesDropdownAsync();
                return View(model);
            }
        }

        // GET: /Supplies/Delete/{id}
        [HttpGet]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Delete(int id)
        {
            var supply = await _supplyService.GetDetailAsync(id);
            if (supply == null) return NotFound();
            return View(supply);
        }

        // POST: /Supplies/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _supplyService.SoftDeleteAsync(id);
            TempData["SuccessMessage"] = "Đã chuyển vật tư vào Thùng rác.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Supplies/Trash
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Trash()
        {
            var trashed = await _supplyService.GetTrashAsync();
            return View(trashed);
        }

        // POST: /Supplies/Restore/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _supplyService.RestoreAsync(id);
            if (!result) return NotFound();

            TempData["SuccessMessage"] = "Khôi phục vật tư thành công.";
            return RedirectToAction(nameof(Trash));
        }

        // Feature 1: GET: /Supplies/AdjustStock/{id}
        [HttpGet]
        [Authorize(Policy = "CanAdjustStock")]
        public async Task<IActionResult> AdjustStock(int id)
        {
            var supply = await _supplyService.GetDetailAsync(id);
            if (supply == null) return NotFound();

            var editVm = await _supplyService.GetForEditAsync(id);
            ViewBag.RowVersionBase64 = editVm?.RowVersionBase64;
            return View(supply);
        }

        // Feature 1: POST: /Supplies/AdjustStock/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanAdjustStock")]
        public async Task<IActionResult> AdjustStock(int id, int stockQuantityChange, string rowVersionBase64)
        {
            if (stockQuantityChange == 0)
            {
                ModelState.AddModelError(string.Empty, "Số lượng thay đổi phải khác 0.");
                var supply = await _supplyService.GetDetailAsync(id);
                ViewBag.RowVersionBase64 = rowVersionBase64;
                return View(supply);
            }

            try
            {
                var result = await _supplyService.AdjustStockAsync(id, stockQuantityChange, rowVersionBase64);
                if (!result) return NotFound();

                TempData["SuccessMessage"] = "Điều chỉnh tồn kho thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var supply = await _supplyService.GetDetailAsync(id);
                ViewBag.RowVersionBase64 = rowVersionBase64;
                return View(supply);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu đã thay đổi bởi người khác. Vui lòng quay lại danh sách và thử lại.");
                var supply = await _supplyService.GetDetailAsync(id);
                ViewBag.RowVersionBase64 = rowVersionBase64;
                return View(supply);
            }
        }

        // Feature 2: POST: /Supplies/UploadImage/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanUploadProductImage")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            var supply = await _supplyService.GetDetailAsync(id);
            if (supply == null) return NotFound();

            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn tệp hình ảnh để tải lên.";
                await _auditLogService.LogAsync("UploadImage", "MedicalSupply", id.ToString(), "Failed", "Không có file nào được tải lên.");
                return RedirectToAction(nameof(Edit), new { id });
            }

            string? oldImagePath = supply.ImagePath;
            string? newImagePath = null;

            try
            {
                // Save new image safely
                newImagePath = await _fileUploadService.SaveProductImageAsync(imageFile);

                // Update database
                var updateResult = await _supplyService.UpdateImagePathAsync(id, newImagePath);
                if (updateResult)
                {
                    // Success! Delete the old image file if it exists
                    if (!string.IsNullOrWhiteSpace(oldImagePath))
                    {
                        _fileUploadService.DeleteImage(oldImagePath);
                    }
                    TempData["SuccessMessage"] = "Tải lên hình ảnh thành công.";
                    await _auditLogService.LogAsync("UploadImage", "MedicalSupply", id.ToString(), "Success", $"Đã thay ảnh thành công: {newImagePath}");
                }
                else
                {
                    // Update failed, delete the newly uploaded file to not leak orphan files
                    _fileUploadService.DeleteImage(newImagePath);
                    TempData["ErrorMessage"] = "Lưu đường dẫn ảnh vào cơ sở dữ liệu thất bại.";
                    await _auditLogService.LogAsync("UploadImage", "MedicalSupply", id.ToString(), "Failed", "Lưu cơ sở dữ liệu thất bại.");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải lên ảnh: {ex.Message}";
                await _auditLogService.LogAsync("UploadImage", "MedicalSupply", id.ToString(), "Failed", $"Tải lên thất bại: {ex.Message}");
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        // Creative Feature: POST: /Supplies/ToggleQuarantine/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageProduct")]
        public async Task<IActionResult> ToggleQuarantine(int id, bool isQuarantined, string? quarantineReason)
        {
            if (isQuarantined && string.IsNullOrWhiteSpace(quarantineReason))
            {
                TempData["ErrorMessage"] = "Yêu cầu cung cấp lý do khi đưa vật tư vào cách ly.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var result = await _supplyService.ToggleQuarantineAsync(id, isQuarantined, quarantineReason);
            if (!result) return NotFound();

            TempData["SuccessMessage"] = isQuarantined ? "Đã đưa vật tư vào cách ly." : "Đã hủy cách ly vật tư.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        private async Task PopulateCategoriesDropdownAsync()
        {
            var categories = await _context.SupplyCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
    }
}
