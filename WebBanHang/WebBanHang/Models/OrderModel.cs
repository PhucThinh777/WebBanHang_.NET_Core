using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class OrderModel
    {
        public long? Id { get; set; }
        public string OrderCode {  get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
    }
}
