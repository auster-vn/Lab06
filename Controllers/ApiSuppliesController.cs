using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using System.Diagnostics;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiSuppliesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiSuppliesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/apisupplies/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supply = await _context.MedicalSupplies
                .AsNoTracking()
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supply == null)
            {
                var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                
                return NotFound(new ProblemDetails
                {
                    Type = "https://example.com/problems/supply-not-found",
                    Title = "Medical Supply Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Vật tư y tế với ID {id} không tồn tại trong hệ thống.",
                    Instance = HttpContext.Request.Path,
                    Extensions = 
                    {
                        { "traceId", traceId },
                        { "errorCode", "SUPPLY_NOT_FOUND" },
                        { "timestamp", DateTimeOffset.UtcNow }
                    }
                });
            }

            return Ok(new
            {
                supply.Id,
                supply.SupplyCode,
                supply.SupplyName,
                CategoryName = supply.Category?.Name ?? "N/A",
                supply.Supplier,
                supply.UnitPrice,
                supply.StockQuantity,
                supply.MinimumStock,
                supply.Description,
                supply.ImagePath,
                supply.IsQuarantined
            });
        }

        // GET: /api/apisupplies/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ModelState.AddModelError("keyword", "Từ khóa tìm kiếm (keyword) không được rỗng hoặc chỉ có khoảng trắng.");
                return ValidationProblem(new ValidationProblemDetails(ModelState)
                {
                    Type = "https://example.com/problems/validation-error",
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Tham số tìm kiếm không hợp lệ.",
                    Instance = HttpContext.Request.Path,
                    Extensions = 
                    {
                        { "traceId", traceId },
                        { "errorCode", "INVALID_SEARCH_KEYWORD" },
                        { "timestamp", DateTimeOffset.UtcNow }
                    }
                });
            }

            if (keyword.Length > 50)
            {
                ModelState.AddModelError("keyword", "Từ khóa tìm kiếm không được vượt quá 50 ký tự.");
                return ValidationProblem(new ValidationProblemDetails(ModelState)
                {
                    Type = "https://example.com/problems/validation-error",
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Tham số tìm kiếm vượt giới hạn độ dài.",
                    Instance = HttpContext.Request.Path,
                    Extensions = 
                    {
                        { "traceId", traceId },
                        { "errorCode", "KEYWORD_TOO_LONG" },
                        { "timestamp", DateTimeOffset.UtcNow }
                    }
                });
            }

            var lowerKeyword = keyword.ToLower();
            var results = await _context.MedicalSupplies
                .AsNoTracking()
                .Include(s => s.Category)
                .Where(s => s.SupplyName.ToLower().Contains(lowerKeyword) || 
                            s.SupplyCode.ToLower().Contains(lowerKeyword) || 
                            (s.Description != null && s.Description.ToLower().Contains(lowerKeyword)))
                .Select(s => new
                {
                    s.Id,
                    s.SupplyCode,
                    s.SupplyName,
                    CategoryName = s.Category != null ? s.Category.Name : "N/A",
                    s.Supplier,
                    s.UnitPrice,
                    s.StockQuantity,
                    s.ImagePath,
                    s.IsQuarantined
                })
                .ToListAsync();

            if (results.Count == 0)
            {
                return NotFound(new ProblemDetails
                {
                    Type = "https://example.com/problems/no-results-found",
                    Title = "No Results Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Không tìm thấy vật tư y tế nào khớp với từ khóa '{keyword}'.",
                    Instance = HttpContext.Request.Path,
                    Extensions = 
                    {
                        { "traceId", traceId },
                        { "errorCode", "NO_RESULTS_FOUND" },
                        { "timestamp", DateTimeOffset.UtcNow }
                    }
                });
            }

            return Ok(results);
        }
    }
}
