using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Controllers
{
	public class LoginController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
