using Microsoft.EntityFrameworkCore;
using MedicalSuppliesCatalog.Lab06.Data;
using MedicalSuppliesCatalog.Lab06.Models;

namespace MedicalSuppliesCatalog.Lab06.Repositories
{
    public class SupplyRepository : ISupplyRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicalSupply>> GetAllReadOnlyAsync()
        {
            return await _context.MedicalSupplies
                .Include(s => s.Category)
                .AsNoTracking()
                .OrderBy(s => s.SupplyName)
                .ToListAsync();
        }

        public async Task<MedicalSupply?> GetByIdAsync(int id)
        {
            return await _context.MedicalSupplies
                .Include(s => s.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<MedicalSupply?> GetByIdWithTrackingAsync(int id)
        {
            return await _context.MedicalSupplies
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<MedicalSupply>> FilterAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.MedicalSupplies
                .Include(s => s.Category)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(s => s.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(s => s.UnitPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(s => s.UnitPrice <= maxPrice.Value);

            return await query.OrderBy(s => (double)s.UnitPrice).ToListAsync();
        }

        public async Task<List<MedicalSupply>> SearchAsync(string? keyword)
        {
            var query = _context.MedicalSupplies
                .Include(s => s.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(s => s.SupplyName.ToLower().Contains(lowerKeyword) || 
                                         s.SupplyCode.ToLower().Contains(lowerKeyword) || 
                                         (s.Description != null && s.Description.ToLower().Contains(lowerKeyword)));
            }

            return await query.OrderBy(s => s.SupplyName).ToListAsync();
        }

        public async Task<List<MedicalSupply>> GetTrashReadOnlyAsync()
        {
            // IgnoreQueryFilters allows querying soft-deleted items (where IsDeleted == true)
            return await _context.MedicalSupplies
                .IgnoreQueryFilters()
                .Include(s => s.Category)
                .AsNoTracking()
                .Where(s => s.IsDeleted)
                .OrderByDescending(s => s.DeletedAt)
                .ToListAsync();
        }

        public async Task<MedicalSupply?> GetByIdFromTrashAsync(int id)
        {
            return await _context.MedicalSupplies
                .IgnoreQueryFilters()
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);
        }

        public async Task AddAsync(MedicalSupply supply)
        {
            await _context.MedicalSupplies.AddAsync(supply);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
