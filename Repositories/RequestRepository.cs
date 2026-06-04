using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public RequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SupplyRequest>> GetAllWithIssuesAsync()
        {
            return await _context.SupplyRequests
                .Include(r => r.SupplyIssues)
                    .ThenInclude(i => i.MedicalSupply)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupplyRequest?> GetByIdAsync(int id)
        {
            return await _context.SupplyRequests
                .Include(r => r.SupplyIssues)
                    .ThenInclude(i => i.MedicalSupply)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(SupplyRequest request)
        {
            await _context.SupplyRequests.AddAsync(request);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
