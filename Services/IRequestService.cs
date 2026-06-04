using MedicalSuppliesCatalog.Lab06.ViewModels;

namespace MedicalSuppliesCatalog.Lab06.Services
{
    public interface IRequestService
    {
        Task CreateRequestAsync(RequestCreateViewModel model);
        Task<List<RequestHistoryViewModel>> GetHistoryAsync();
    }
}
