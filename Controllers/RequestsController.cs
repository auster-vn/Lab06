using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Services;
using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    /// <summary>
    /// Quản lý yêu cầu cấp phát vật tư.
    /// POST /Requests/Create tạo SupplyRequest + SupplyIssue + trừ Stock trong transaction.
    /// </summary>
    public class RequestsController : Controller
    {
        private readonly IRequestService _requestService;
        private readonly ApplicationDbContext _context;

        public RequestsController(IRequestService requestService, ApplicationDbContext context)
        {
            _requestService = requestService;
            _context = context;
        }

        // GET /Requests/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSuppliesDropdownAsync();
            return View(new RequestCreateViewModel());
        }

        // POST /Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequestCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSuppliesDropdownAsync();
                return View(model);
            }

            try
            {
                await _requestService.CreateRequestAsync(model);
                TempData["SuccessMessage"] = "Supply request created successfully!";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSuppliesDropdownAsync();
                return View(model);
            }
        }

        // GET /Requests/History — Trang lịch sử cấp phát (dùng AsNoTracking)
        public async Task<IActionResult> History()
        {
            var history = await _requestService.GetHistoryAsync();
            return View(history);
        }

        private async Task PopulateSuppliesDropdownAsync()
        {
            var supplies = await _context.MedicalSupplies
                .AsNoTracking()
                .OrderBy(s => s.SupplyName)
                .ToListAsync();

            ViewBag.Supplies = supplies.Select(s =>
                new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.SupplyName} (Stock: {s.StockQuantity}) - {s.UnitPrice:N0} VND"
                }).ToList();
        }
    }
}
