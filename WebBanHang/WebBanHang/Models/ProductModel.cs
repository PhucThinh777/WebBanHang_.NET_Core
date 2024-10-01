using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanHang.Repository.Validation;

namespace WebBanHang.Models
{
    public class ProductModel
	{
		[Key]
		public long Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
		public string Description { get; set; }
		public string Slug { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        //[Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

		public string Image { get; set; }

		[Required, Range(1,int.MaxValue, ErrorMessage ="Chọn 1 thương hiệu")]
		public long BrandId { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn 1 danh mục")]
        public long CategoryId { get; set; }
		public BrandModel Brand { get; set; }
		public CategoryModel Category { get; set; }
		public RatingModel Ratings { get; set; }

		[NotMapped]
        //[Required(ErrorMessage = "Yêu cầu chọn ảnh")]
        [FileExtension("jpg", "jpeg", "png", ErrorMessage = "Chỉ chấp nhận các định dạng: jpg, jpeg, png")]
        public IFormFile? ImageUpload { get; set; }
    }
}
