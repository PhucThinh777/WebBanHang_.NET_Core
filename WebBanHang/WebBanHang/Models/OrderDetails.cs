using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class OrderDetails
    {
        public long Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public long ProductId {  get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public long PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public PaymentModel Payment { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
