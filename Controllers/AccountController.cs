using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MedicalSuppliesCatalog.Lab06.Models;
using MedicalSuppliesCatalog.Lab06.Services;
using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLogService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new AccountRegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Default registered users are added to "User" role
                await _userManager.AddToRoleAsync(user, "User");
                
                await _auditLogService.LogAsync("Register", "ApplicationUser", user.Id, "Success", $"Đăng ký tài khoản mới: {model.Email}");
                
                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await _auditLogService.LogAsync("Register", "ApplicationUser", null, "Failed", $"Đăng ký tài khoản thất bại cho: {model.Email}");
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new AccountLoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await _auditLogService.LogAsync("Login", "ApplicationUser", model.Email, "Success", $"Đăng nhập thành công: {model.Email}");
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            await _auditLogService.LogAsync("Login", "ApplicationUser", model.Email, "Failed", $"Đăng nhập thất bại (sai mật khẩu): {model.Email}");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var email = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            await _auditLogService.LogAsync("Logout", "ApplicationUser", email, "Success", $"Đăng xuất thành công: {email}");
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AccessDenied()
        {
            var email = User.Identity?.Name;
            var path = HttpContext.Request.Path;
            await _auditLogService.LogAsync("AccessDenied", "Authorization", email, "Failed", $"Bị từ chối truy cập đường dẫn: {path}");
            return View();
        }
    }
}
