using System.ComponentModel.DataAnnotations;

namespace MedicalSuppliesCatalog.Lab06.ViewModels
{
    public class AccountRegisterViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
