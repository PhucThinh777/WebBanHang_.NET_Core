using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanHang.Repository.Validation;
using static System.Formats.Asn1.AsnWriter;

namespace WebBanHang.Models
{
    public class ContactModel
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tiêu đề")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập map")]
        public string Map {  get; set; }
        public string Logo { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
        public string Phone { get; set; }
        [NotMapped]
        [FileExtension("jpg", "jpeg", "png", ErrorMessage = "Chỉ chấp nhận các định dạng: jpg, jpeg, png")]
        public IFormFile? ImageUpload { get; set; }

    }
}
