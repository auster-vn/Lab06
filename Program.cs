using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;
using MedicalSuppliesCatalog.Lab06.Repositories;
using MedicalSuppliesCatalog.Lab06.Services;
using MedicalSuppliesCatalog.Lab06.Options;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/lab06-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Medical Supplies Lab06 application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/lab06-.txt", rollingInterval: RollingInterval.Day));

    // 1. Add MVC Controllers and Views
    builder.Services.AddControllersWithViews();

    // Options Pattern
    builder.Services.Configure<AppSettings>(
        builder.Configuration.GetSection("AppSettings"));

    // 2. Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 3. Add Identity services
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // 4. Configure Application Cookie
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

    // 5. Register HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    // 6. Register Repositories and Services (DI)
    builder.Services.AddScoped<ISupplyRepository, SupplyRepository>();
    builder.Services.AddScoped<IRequestRepository, RequestRepository>();
    builder.Services.AddScoped<ISupplyService, SupplyService>();
    builder.Services.AddScoped<IRequestService, RequestService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IFileUploadService, FileUploadService>();

    // 7. Define Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("CanViewProduct", policy => 
            policy.RequireRole("Admin", "Staff", "User"));
        
        options.AddPolicy("CanManageProduct", policy => 
            policy.RequireRole("Admin"));
        
        options.AddPolicy("CanViewAuditLog", policy => 
            policy.RequireRole("Admin"));
        
        options.AddPolicy("CanUploadProductImage", policy => 
            policy.RequireRole("Admin"));
        
        options.AddPolicy("CanAdjustStock", policy => 
            policy.RequireRole("Admin", "Staff"));
    });

    // 8. Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."), tags: new[] { "live" })
        .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready" });

    // 9. ProblemDetails (standardized API error format RFC 7807)
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
        };
    });

    var app = builder.Build();

    // 10. Automatically run migrations and seed database on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrated successfully.");

        // Seed Identity roles, users and initial items
        await DbInitializer.SeedIdentityAsync(scope.ServiceProvider);
        Log.Information("Database seeded successfully.");
    }

    // 11. Exception Handling
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

    app.UseStaticFiles();
    app.UseRouting();

    // Authentication before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 12. Map Health Check endpoints
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            
            var totalMemoryMb = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);
            var overallStatus = report.Status == HealthStatus.Healthy ? "Khỏe mạnh (Healthy)" : (report.Status == HealthStatus.Degraded ? "Suy giảm (Degraded)" : "Có lỗi (Unhealthy)");
            var overallStatusColorClass = report.Status == HealthStatus.Healthy ? "success" : (report.Status == HealthStatus.Degraded ? "warning" : "danger");
            var overallStatusIcon = report.Status == HealthStatus.Healthy ? "bi-check-circle-fill" : (report.Status == HealthStatus.Degraded ? "bi-exclamation-triangle-fill" : "bi-x-circle-fill");
            var totalDurationMs = report.TotalDuration.TotalMilliseconds;
            
            var osVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var machineName = Environment.MachineName;
            var serverTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var checksHtml = "";
            foreach (var entry in report.Entries)
            {
                var checkName = entry.Key == "self" ? "Ứng dụng (App)" : (entry.Key == "database" ? "Cơ sở dữ liệu (SQLite)" : entry.Key);
                var checkStatus = entry.Value.Status == HealthStatus.Healthy ? "Tốt (Healthy)" : (entry.Value.Status == HealthStatus.Degraded ? "Cảnh báo (Degraded)" : "Lỗi (Unhealthy)");
                var checkStatusColorClass = entry.Value.Status == HealthStatus.Healthy ? "success" : (entry.Value.Status == HealthStatus.Degraded ? "warning" : "danger");
                var checkDesc = entry.Value.Description ?? "Hệ thống hoạt động bình thường.";
                var durationMs = entry.Value.Duration.TotalMilliseconds;

                checksHtml += $@"
                <div class=""p-3 rounded-3 border d-flex justify-content-between align-items-center mb-3 hover-card transition-all"">
                    <div class=""d-flex align-items-center gap-3"">
                        <div class=""status-indicator bg-{checkStatusColorClass} pulse-light-{checkStatusColorClass}""></div>
                        <div>
                            <h5 class=""fw-bold mb-0 text-dark"">{checkName}</h5>
                            <small class=""text-muted d-block"">{checkDesc}</small>
                        </div>
                    </div>
                    <div class=""text-end"">
                        <span class=""badge bg-{checkStatusColorClass} text-white px-3 py-2 rounded-pill fs-7 fw-bold d-block mb-1"">
                            {checkStatus}
                        </span>
                        <small class=""text-muted fw-mono"">{durationMs:F2} ms</small>
                    </div>
                </div>";
            }

            var html = @"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Trạng thái Hệ thống (Health Checks)</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css"" rel=""stylesheet"">
    <style>
        body {
            background-color: #f8f9fa;
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
        }
        .bg-gradient-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
        }
        .bg-gradient-warning {
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
        }
        .bg-gradient-danger {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
        }
        .pulse-animation {
            animation: pulseHeart 1.5s infinite;
            display: inline-block;
        }
        @keyframes pulseHeart {
            0% { transform: scale(1); }
            50% { transform: scale(1.15); }
            100% { transform: scale(1); }
        }
        .status-icon-glow {
            filter: drop-shadow(0 0 15px rgba(255, 255, 255, 0.6));
            animation: rotateGlow 3s infinite alternate;
        }
        @keyframes rotateGlow {
            0% { transform: scale(1); }
            100% { transform: scale(1.05); }
        }
        .blob-bg {
            position: absolute;
            border-radius: 50%;
            filter: blur(50px);
            opacity: 0.25;
            z-index: 0;
        }
        .blob-1 {
            width: 150px;
            height: 150px;
            background-color: #ffffff;
            top: -30px;
            right: -30px;
        }
        .blob-2 {
            width: 250px;
            height: 250px;
            background-color: #fef08a;
            bottom: -80px;
            left: 20%;
        }
        .z-index-1 {
            z-index: 1;
        }
        .status-indicator {
            width: 14px;
            height: 14px;
            border-radius: 50%;
        }
        .pulse-light-success {
            box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
            animation: pulseLightS 2s infinite;
        }
        .pulse-light-warning {
            box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.7);
            animation: pulseLightW 2s infinite;
        }
        .pulse-light-danger {
            box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7);
            animation: pulseLightD 2s infinite;
        }
        @keyframes pulseLightS {
            70% { box-shadow: 0 0 0 8px rgba(16, 185, 129, 0); }
            100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
        }
        @keyframes pulseLightW {
            70% { box-shadow: 0 0 0 8px rgba(245, 158, 11, 0); }
            100% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0); }
        }
        @keyframes pulseLightD {
            70% { box-shadow: 0 0 0 8px rgba(239, 68, 68, 0); }
            100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
        }
        .hover-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
            border-color: #a7f3d0 !important;
        }
        .transition-all {
            transition: all 0.25s ease-in-out;
        }
        .fw-black {
            font-weight: 900;
        }
        .fs-7 {
            font-size: 0.85rem;
        }
        .fw-mono {
            font-family: SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        }
    </style>
</head>
<body>
    <header class=""bg-white border-bottom shadow-sm"">
        <div class=""container py-3 d-flex justify-content-between align-items-center"">
            <a href=""/"" class=""d-flex align-items-center text-dark text-decoration-none"">
                <i class=""bi bi-hospital text-primary fs-3 me-2""></i>
                <span class=""fs-4 fw-bold"">Medical Supplies Catalog</span>
            </a>
            <a href=""/"" class=""btn btn-outline-primary btn-sm"">
                <i class=""bi bi-arrow-left""></i> Quay lại trang chủ
            </a>
        </div>
    </header>

    <div class=""container py-5"">
        <div class=""row mb-4"">
            <div class=""col-12 text-center text-md-start"">
                <h1 class=""display-5 fw-bold text-dark mb-1"">
                    <i class=""bi bi-heart-pulse text-danger pulse-animation""></i> Trạng Thái Hệ Thống
                </h1>
                <p class=""text-muted fs-5"">Giám sát sức khỏe ứng dụng, cơ sở dữ liệu và tài nguyên máy chủ theo thời gian thực.</p>
            </div>
        </div>

        <div class=""row mb-5"">
            <div class=""col-12"">
                <div class=""card border-0 shadow-lg overflow-hidden position-relative status-banner bg-gradient-{overallStatusColorClass}"">
                    <div class=""card-body p-5 text-white position-relative z-index-1"">
                        <div class=""row align-items-center"">
                            <div class=""col-md-8 text-center text-md-start"">
                                <span class=""badge bg-white bg-opacity-20 text-white px-3 py-2 rounded-pill fs-6 mb-3 border border-white border-opacity-10"">
                                    <i class=""bi bi-shield-check""></i> RFC 7807 Standard Active
                                </span>
                                <h2 class=""display-4 fw-black mb-2"">{overallStatus}</h2>
                                <p class=""fs-5 opacity-90 mb-0"">
                                    Tổng thời gian phản hồi: <strong class=""fs-4 text-warning"">{totalDurationMs} ms</strong>
                                </p>
                            </div>
                            <div class=""col-md-4 text-center text-md-end mt-4 mt-md-0"">
                                <i class=""bi {overallStatusIcon} display-1 status-icon-glow""></i>
                            </div>
                        </div>
                    </div>
                    <div class=""blob-bg blob-1""></div>
                    <div class=""blob-bg blob-2""></div>
                </div>
            </div>
        </div>

        <div class=""row g-4"">
            <div class=""col-lg-7"">
                <div class=""card border-0 shadow-sm h-100 rounded-3"">
                    <div class=""card-header bg-transparent border-0 pt-4 px-4 d-flex justify-content-between align-items-center"">
                        <h4 class=""card-title fw-bold text-dark mb-0"">
                            <i class=""bi bi-cpu-fill text-primary me-2""></i> Thành phần Hệ thống
                        </h4>
                        <span class=""badge bg-light text-dark px-3 py-2 rounded-pill border"">{entriesCount} giám sát</span>
                    </div>
                    <div class=""card-body p-4"">
                        <div class=""d-flex flex-column"">
                            {checksHtml}
                        </div>
                    </div>
                </div>
            </div>

            <div class=""col-lg-5"">
                <div class=""card border-0 shadow-sm h-100 rounded-3"">
                    <div class=""card-header bg-transparent border-0 pt-4 px-4"">
                        <h4 class=""card-title fw-bold text-dark mb-0"">
                            <i class=""bi bi-hdd-network-fill text-primary me-2""></i> Thông số Máy chủ
                        </h4>
                    </div>
                    <div class=""card-body p-4"">
                        <div class=""table-responsive"">
                            <table class=""table table-borderless align-middle mb-0"">
                                <tbody>
                                    <tr class=""border-bottom"">
                                        <td class=""text-muted py-3 px-0"">Hệ điều hành</td>
                                        <td class=""text-end fw-bold text-dark py-3 px-0 text-truncate"" style=""max-width: 250px;"" title=""{osVersion}"">
                                            {osVersion}
                                        </td>
                                    </tr>
                                    <tr class=""border-bottom"">
                                        <td class=""text-muted py-3 px-0"">Tên máy chủ (Host)</td>
                                        <td class=""text-end fw-bold text-dark py-3 px-0"">{machineName}</td>
                                    </tr>
                                    <tr class=""border-bottom"">
                                        <td class=""text-muted py-3 px-0"">Bộ nhớ Tiến trình (RAM)</td>
                                        <td class=""text-end fw-bold text-dark py-3 px-0"">
                                            <i class=""bi bi-memory me-1 text-primary""></i> {totalMemoryMb} MB
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""text-muted py-3 px-0"">Giờ máy chủ</td>
                                        <td class=""text-end fw-bold text-dark py-3 px-0"">
                                            <i class=""bi bi-clock me-1 text-primary""></i> {serverTime}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <footer class=""border-top text-muted mt-5 bg-white"">
        <div class=""container py-3"">
            &copy; 2026 - Medical Supplies Catalog — ASP.NET Core Health Checks
        </div>
    </footer>
</body>
</html>";

            html = html
                .Replace("{overallStatus}", overallStatus)
                .Replace("{overallStatusColorClass}", overallStatusColorClass)
                .Replace("{overallStatusIcon}", overallStatusIcon)
                .Replace("{totalDurationMs}", totalDurationMs.ToString("F2"))
                .Replace("{checksHtml}", checksHtml)
                .Replace("{osVersion}", osVersion)
                .Replace("{machineName}", machineName)
                .Replace("{totalMemoryMb}", totalMemoryMb.ToString("F2"))
                .Replace("{serverTime}", serverTime)
                .Replace("{entriesCount}", report.Entries.Count.ToString());

            await context.Response.WriteAsync(html);
        }
    });

    // 13. Map Routing
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
