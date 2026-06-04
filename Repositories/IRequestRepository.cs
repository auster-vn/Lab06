using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Repositories
{
    public interface IRequestRepository
    {
        Task<List<SupplyRequest>> GetAllWithIssuesAsync();
        Task<SupplyRequest?> GetByIdAsync(int id);
        Task AddAsync(SupplyRequest request);
        Task SaveChangesAsync();
    }
}
