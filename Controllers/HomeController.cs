using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalSuppliesCatalog.Lab06.Services;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISupplyService _supplyService;

        public HomeController(ISupplyService supplyService)
        {
            _supplyService = supplyService;
        }

        // GET: / (Dashboard)
        public async Task<IActionResult> Index()
        {
            var vm = await _supplyService.GetDashboardAsync();
            return View(vm);
        }

        // GET: /Home/Error
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }

        // GET: /Home/StatusCode?code=...
        [AllowAnonymous]
        [ActionName("StatusCode")]
        public IActionResult StatusCodePage(int code)
        {
            ViewData["StatusCode"] = code;
            return View("StatusCode");
        }
    }
}
