using Microsoft.AspNetCore.Http;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public interface IFileUploadService
    {
        Task<string> SaveProductImageAsync(IFormFile file);
        void DeleteImage(string imagePath);
    }
}
