using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập")]
        public int Quantity { get; set; }
        public long ProductId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
