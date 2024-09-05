using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
	public class UserModel
	{
		public int Id {  get; set; }

		[Required(ErrorMessage ="Hãy nhập Username")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Hãy nhập Email")]
		public string Email { get; set; }

		[DataType(DataType.Password),Required(ErrorMessage ="Hãy nhập Password")]
		public string Password { get; set; }
	}
}
