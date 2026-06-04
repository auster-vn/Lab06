using System.ComponentModel.DataAnnotations;

namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class SupplyEditViewModel : SupplyCreateViewModel
    {
        public int Id { get; set; }

        public string? ImagePath { get; set; }

        /// <summary>
        /// RowVersion dưới dạng Base64 string — đưa vào hidden field ở form Edit
        /// để server phát hiện concurrent edits
        /// </summary>
        public string RowVersionBase64 { get; set; } = string.Empty;
    }
}
