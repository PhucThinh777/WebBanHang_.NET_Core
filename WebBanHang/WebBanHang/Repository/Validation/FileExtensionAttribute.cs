using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace WebBanHang.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;

        public FileExtensionAttribute(params string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                // Lấy phần đuôi của tên tệp và chuyển thành chữ thường
                var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

                // So sánh với danh sách các định dạng được phép
                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"Chỉ chấp nhận các định dạng: {string.Join(", ", _allowedExtensions)}");
                }
            }

            return ValidationResult.Success;
        }

    }
}
