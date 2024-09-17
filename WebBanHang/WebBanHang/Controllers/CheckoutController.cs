using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Areas.Admin.Repository;
using WebBanHang.Models;
using WebBanHang.Repository;

namespace WebBanHang.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;

        public CheckoutController(IEmailSender emailSender, DataContext context)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}
        public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
            {
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;
				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach(var cart in cartItems)
				{
					var orderdetails = new OrderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = ordercode;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Price = cart.Price;
					orderdetails.Quantity = cart.Quantity;
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("Cart");
                //Gửi Email
                var receiver = userEmail;
                var subject = "Đặt hàng thành công";
                var message = "Đặt hàng thành công, Cảm ơn bạn đã sử dụng dịch vụ bên chúng tôi.\n" +
							  "Thông tin chi tiết: \n" + 
							  "Tài khoản: " + userEmail + "\n" + 
							  "Mã đơn hàng: " + ordercode;

                await _emailSender.SendEmailAsync(receiver, subject, message);
                TempData["success"] = "Checkout thành công vui lòng chờ duyệt đơn hàng";
				return RedirectToAction("Index", "Cart");
            }
            return View();
		}
	}
}
