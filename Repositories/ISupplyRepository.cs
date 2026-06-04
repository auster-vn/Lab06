using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Repositories
{
    public interface ISupplyRepository
    {
        Task<List<MedicalSupply>> GetAllReadOnlyAsync();
        Task<MedicalSupply?> GetByIdAsync(int id);
        Task<MedicalSupply?> GetByIdWithTrackingAsync(int id);
        Task<List<MedicalSupply>> FilterAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
        Task<List<MedicalSupply>> SearchAsync(string? keyword);
        Task<List<MedicalSupply>> GetTrashReadOnlyAsync();
        Task<MedicalSupply?> GetByIdFromTrashAsync(int id);
        Task AddAsync(MedicalSupply supply);
        Task SaveChangesAsync();
    }
}
