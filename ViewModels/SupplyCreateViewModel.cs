using System.ComponentModel.DataAnnotations;

namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class SupplyCreateViewModel
    {
        [Required(ErrorMessage = "Mã vật tư (Supply Code) là bắt buộc.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mã vật tư phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Mã vật tư chỉ cho phép chữ in hoa, chữ số và dấu gạch ngang.")]
        public string SupplyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên vật tư là bắt buộc.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên vật tư phải từ 3 đến 200 ký tự.")]
        public string SupplyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục hợp lệ.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Nhà cung cấp không vượt quá 200 ký tự.")]
        public string Supplier { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "Đơn giá phải từ 0 đến 999,999,999 VND.")]
        public decimal UnitPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 1,000,000.")]
        public int StockQuantity { get; set; }

        [Range(0, 100000, ErrorMessage = "Tồn kho tối thiểu phải từ 0 đến 100,000.")]
        public int MinimumStock { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        public bool IsQuarantined { get; set; }

        [StringLength(200, ErrorMessage = "Lý do cách ly không vượt quá 200 ký tự.")]
        public string? QuarantineReason { get; set; }
    }
}
