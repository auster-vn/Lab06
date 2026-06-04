using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveProductImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or not provided.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type is not allowed. Only {string.Join(", ", AllowedExtensions)} are permitted.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("File size exceeds the 2MB limit.");
            }

            // Generate unique name to prevent path traversal and overwriting
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "products");
            
            // Ensure directory exists
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/products/{uniqueFileName}";
        }

        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            // Normalize and resolve path safely
            var relativePath = imagePath.TrimStart('/');
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            try
            {
                // Ensure the path stays within wwwroot/uploads/products to prevent path traversal deletes
                var canonicalPath = Path.GetFullPath(absolutePath);
                var canonicalUploadsDir = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", "products"));

                if (canonicalPath.StartsWith(canonicalUploadsDir, StringComparison.OrdinalIgnoreCase) && File.Exists(canonicalPath))
                {
                    File.Delete(canonicalPath);
                }
            }
            catch
            {
                // Non-critical operation, suppress errors to prevent application crash on cleanup
            }
        }
    }
}
