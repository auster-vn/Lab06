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
