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
        Predicate = check => check.Tags.Contains("ready")
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
