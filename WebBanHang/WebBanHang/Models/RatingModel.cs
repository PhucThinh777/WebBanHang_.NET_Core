using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }
        public long ProductId { get; set; }
        [Required(ErrorMessage = "Nhập đánh giá của bạn")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Nhập tên của bạn")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Nhập email của bạn")]
        public string Email { get; set; }
        public string Star { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
