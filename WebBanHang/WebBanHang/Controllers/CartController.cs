using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Areas.Admin.Repository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;
using WebBanHang.Repository;

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly PaypalClient _paypalClient;
        private readonly IEmailSender _emailSender;

        public CartController(DataContext context, PaypalClient paypalClient, IEmailSender emailSender)
        {
            _dataContext = context;
            _paypalClient = paypalClient;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
            };
            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(cartVM);
        }

        public IActionResult IndexCOD()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
            };
            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(cartVM);
        }

        public IActionResult IndexPaypal()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
            };
            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(cartVM);
        }

        public async Task<IActionResult> Add(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItems == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
            {
                cartItems.Quantity += 1;
            }
            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Sản phẩm đã được thêm vào giỏ hàng";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.Remove(cartItem);
                }
                HttpContext.Session.SetJson("Cart", cart);
            }

            //TempData["success"] = "Giảm số lượng";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity >= 1)
            {
                ++cartItem.Quantity;

            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            //TempData["success"] = "Tăng thành công";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            cart.RemoveAll(p => p.ProductId == Id);
            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Đã xoá sản phẩm khỏi giỏ hàng";
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["success"] = "Đã xoá toàn bộ giỏ hàng";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string fullName, string phone, string address, long paymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tạo mã đơn hàng
            var ordercode = Guid.NewGuid().ToString();

            // Tạo đối tượng đơn hàng
            var orderItem = new OrderModel
            {
                OrderCode = ordercode,
                UserName = userEmail,
                Status = 1,
                CreatedDate = DateTime.Now,
            };

            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            foreach (var cart in cartItems)
            {
                var orderdetails = new OrderDetails
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity,
                    FullName = fullName,
                    Phone = phone,
                    Address = address,
                    PaymentId = paymentId
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
                          $"Mã đơn hàng: {ordercode}\n" +
                          $"Tên: {fullName}\n" +
                          $"Điện thoại: {phone}\n" +
                          $"Địa chỉ: {address}\n" +
                          $"Thanh toán: Khi nhận hàng";

            await _emailSender.SendEmailAsync(receiver, subject, message);

            TempData["success"] = "Đặt hàng thành công, vui lòng chờ duyệt đơn hàng";

            return RedirectToAction("CheckoutSuccess");
        }


        #region Paypal payment

        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (!cartItems.Any())
            {
                return BadRequest(new { message = "The cart is empty." });
            }

            var totalAmount = cartItems.Sum(p => p.Quantity * p.Price).ToString("F2");
            var currency = "USD";
            var referenceOrderId = "Order" + DateTime.Now.Ticks;

            try
            {
                var response = await _paypalClient.CreateOrder(totalAmount, currency, referenceOrderId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, [FromBody] OrderDetails model)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
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

                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

                foreach (var cart in cartItems)
                {
                    var orderDetails = new OrderDetails
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity,
                        FullName = model.FullName,
                        Phone = model.Phone,
                        Address = model.Address,
                        PaymentId = 2
                    };

                    _dataContext.Add(orderDetails);
                }

                await _dataContext.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");

                // Gửi email
                var subject = "Đặt hàng thành công";
                var message = $"Đặt hàng thành công, Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.\n" +
                              $"Chi tiết đơn hàng:\n" +
                              $"Tài khoản: {userEmail}\n" +
                              $"Mã đơn hàng: {ordercode}\n" +
                              $"Tên: {model.FullName}\n" +
                              $"Điện thoại: {model.Phone}\n" +
                              $"Địa chỉ: {model.Address}\n" +
                              $"Thanh toán: PayPal";

                await _emailSender.SendEmailAsync(userEmail, subject, message);

                TempData["success"] = "Thanh toán thành công, vui lòng chờ duyệt đơn hàng";

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }


        public IActionResult PaymentSuccess()
        {
            return View();
        }
        public IActionResult CheckoutSuccess()
        {
            return View();
        }
        #endregion
    }
}
