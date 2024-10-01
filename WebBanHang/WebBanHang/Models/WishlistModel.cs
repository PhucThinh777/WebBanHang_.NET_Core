using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class WishlistModel
    {
        [Key]
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
