using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class PaymentModel
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }

        public string Status { get; set; }
    }
}
