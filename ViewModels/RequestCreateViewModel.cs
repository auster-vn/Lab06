using System.ComponentModel.DataAnnotations;

namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class RequestCreateViewModel
    {
        [Required(ErrorMessage = "Requester name is required.")]
        [StringLength(150)]
        public string RequesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a supply.")]
        public int MedicalSupplyId { get; set; }

        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
