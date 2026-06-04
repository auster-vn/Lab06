using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Data
{
    public static class DbInitializer
    {
        public static async Task SeedIdentityAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roles = { "Admin", "Staff", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Users
            await CreateUser(userManager, "admin@shop.test", "Admin@123", "Admin");
            await CreateUser(userManager, "staff@shop.test", "Staff@123", "Staff");
            await CreateUser(userManager, "user@shop.test", "User@123", "User");

            // 3. Seed Audit Logs
            var context = services.GetRequiredService<ApplicationDbContext>();
            if (!context.AuditLogs.Any())
            {
                var now = DateTime.UtcNow;
                var logs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        Action = "Login",
                        EntityName = "ApplicationUser",
                        EntityId = "admin@shop.test",
                        UserName = "admin@shop.test",
                        IpAddress = "192.168.1.10",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-30),
                        Note = "Admin logged in successfully."
                    },
                    new AuditLog
                    {
                        Action = "EditSupply",
                        EntityName = "MedicalSupply",
                        EntityId = "1",
                        UserName = "admin@shop.test",
                        IpAddress = "192.168.1.10",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-25),
                        Note = "Admin updated Surgical Mask description and unit price."
                    },
                    new AuditLog
                    {
                        Action = "AdjustStock",
                        EntityName = "MedicalSupply",
                        EntityId = "2",
                        UserName = "staff@shop.test",
                        IpAddress = "192.168.1.15",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-20),
                        Note = "Staff adjusted stock for Disposable Gloves (+50)."
                    },
                    new AuditLog
                    {
                        Action = "Quarantine",
                        EntityName = "MedicalSupply",
                        EntityId = "3",
                        UserName = "admin@shop.test",
                        IpAddress = "192.168.1.10",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-15),
                        Note = "Admin quarantined Digital Thermometer (Reason: Sensor calibration failure)."
                    },
                    new AuditLog
                    {
                        Action = "UploadImage",
                        EntityName = "MedicalSupply",
                        EntityId = "1",
                        UserName = "admin@shop.test",
                        IpAddress = "192.168.1.10",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-10),
                        Note = "Admin uploaded surgical_mask_v2.png."
                    },
                    new AuditLog
                    {
                        Action = "DeleteSupply",
                        EntityName = "MedicalSupply",
                        EntityId = "5",
                        UserName = "unauthorized@shop.test",
                        IpAddress = "203.0.113.5",
                        Result = "Success",
                        CreatedAt = now.AddMinutes(-5),
                        Note = "Attempted unauthorized hard delete of blood pressure monitor.",
                        Hash = "INVALID_HASH_SIMULATED_DB_TAMPERING"
                    }
                };

                foreach (var log in logs)
                {
                    if (log.Hash == null)
                    {
                        log.Hash = CalculateHash(log);
                    }
                    context.AuditLogs.Add(log);
                }

                await context.SaveChangesAsync();
            }
        }

        private static string CalculateHash(AuditLog log)
        {
            var data = $"{log.Action}|{log.EntityName}|{log.EntityId}|{log.UserName}|{log.IpAddress}|{log.Result}|{log.CreatedAt.Ticks}|MedicalSuppliesCatalogSecureKey2026";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static async Task CreateUser(UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = $"{role} Demo"
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
