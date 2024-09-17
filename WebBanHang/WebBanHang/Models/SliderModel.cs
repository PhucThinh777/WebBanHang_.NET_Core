using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanHang.Repository.Validation;

namespace WebBanHang.Models
{
    public class SliderModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        [FileExtension("jpg", "jpeg", "png", ErrorMessage = "Chỉ chấp nhận các định dạng: jpg, jpeg, png")]
        public IFormFile[]? UploadSlider { get; set; }
    }
}
