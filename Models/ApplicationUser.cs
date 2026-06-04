using Microsoft.AspNetCore.Identity;

namespace MedicalSuppliesCatalog.Lab06.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
