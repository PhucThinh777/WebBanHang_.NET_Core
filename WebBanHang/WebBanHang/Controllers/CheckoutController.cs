using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Models;
using WebBanHang.Repository;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using WebBanHang.Areas.Admin.Repository;

namespace WebBanHang.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly PaypalClient _paypalClient;

        public CheckoutController(IEmailSender emailSender, DataContext context, PaypalClient paypalClient)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _paypalClient = paypalClient;
        }



        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ordercode = Guid.NewGuid().ToString();
            var orderItem = new OrderModel
            {
                OrderCode = ordercode,
                UserName = userEmail,
                Status = 1,
                CreatedDate = DateTime.Now
            };

            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // Lấy giỏ hàng từ session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            foreach (var cart in cartItems)
            {
                var orderdetails = new OrderDetails
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };

                _dataContext.Add(orderdetails);
            }

            await _dataContext.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            // Gửi Email xác nhận đơn hàng
            var receiver = userEmail;
            var subject = "Đặt hàng thành công";
            var message = $"Đặt hàng thành công, Cảm ơn bạn đã sử dụng dịch vụ bên chúng tôi.\n" +
                          $"Thông tin chi tiết: \n" +
                          $"Tài khoản: {userEmail}\n" +
                          $"Mã đơn hàng: {ordercode}";

            await _emailSender.SendEmailAsync(receiver, subject, message);

            TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";

            return RedirectToAction("Index", "Cart");
        }
    }
}
