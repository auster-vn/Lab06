using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public interface ISupplyService
    {
        Task<List<SupplyListItemViewModel>> GetActiveSuppliesAsync();
        Task<SupplyDetailViewModel?> GetDetailAsync(int id);
        Task<SupplyEditViewModel?> GetForEditAsync(int id);
        Task CreateAsync(SupplyCreateViewModel model);
        Task<bool> EditAsync(SupplyEditViewModel model);
        Task<bool> SoftDeleteAsync(int id);
        Task<List<SupplyTrashItemViewModel>> GetTrashAsync();
        Task<bool> RestoreAsync(int id);
        Task<DashboardViewModel> GetDashboardAsync();
        Task<List<SupplyListItemViewModel>> SearchSuppliesAsync(string? keyword);
        Task<bool> AdjustStockAsync(int id, int stockQuantityChange, string rowVersionStr);
        Task<bool> ToggleQuarantineAsync(int id, bool isQuarantined, string? reason);
        Task<bool> UpdateImagePathAsync(int id, string? imagePath);
    }
}
