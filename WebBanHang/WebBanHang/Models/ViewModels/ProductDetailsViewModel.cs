using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }
        [Required(ErrorMessage = "Nhập đánh giá của bạn")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Nhập tên của bạn")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Nhập email của bạn")]
        public string Email { get; set; }
        public string Star { get; set; }
    }
}
