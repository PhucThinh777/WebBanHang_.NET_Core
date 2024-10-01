using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.Areas.Admin.Repository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;
using WebBanHang.Repository;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        #region Change Rate
        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            string url = $"https://www.google.com/finance/quote/{fromCurrency}-{toCurrency}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Tìm chuỗi tỷ giá trong thẻ div
                    int startIndex = responseBody.IndexOf("<div class=\"YMlKec fxKbKc\">") + "<div class=\"YMlKec fxKbKc\">".Length;
                    int endIndex = responseBody.IndexOf("</div>", startIndex);
                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        string rateString = responseBody.Substring(startIndex, endIndex - startIndex);
                        rateString = rateString.Replace(".", "").Replace(",", "."); // Chuyển đổi định dạng

                        if (decimal.TryParse(rateString, out decimal rate))
                        {
                            return rate;
                        }
                    }
                }
            }
            throw new Exception("Không thể lấy tỷ giá từ Google Finance");
        }

        #endregion

        #region Index Page
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
        #endregion

        #region Function
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
            ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
            {
                ++cartItem.Quantity;

            }
            else
            {
                cartItem.Quantity = product.Quantity;
                TempData["success"] = "Sản phẩm đã đạt mức tối đa";
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
        #endregion

        #region Checkout

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
                var orderdetails = new OrderDetails();

                orderdetails.UserName = userEmail;
                orderdetails.OrderCode = ordercode;
                orderdetails.ProductId = cart.ProductId;
                orderdetails.Price = cart.Price;
                orderdetails.Quantity = cart.Quantity;
                orderdetails.FullName = fullName;
                orderdetails.Phone = phone;
                orderdetails.Address = address;
                orderdetails.PaymentId = paymentId;

                //update product quantity
                var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                product.Quantity -= cart.Quantity;
                product.Sold += cart.Quantity;
                _dataContext.Update(product);
                _dataContext.Add(orderdetails);
                _dataContext.SaveChanges();
            }
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
        #endregion

        #region Paypal Payment

        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (!cartItems.Any())
            {
                return BadRequest(new { message = "The cart is empty." });
            }

            try
            {
                // Lấy tỷ giá hối đoái USD/VND từ Google Finance
                decimal exchangeRate = await GetExchangeRateAsync("USD", "VND");

                // Tính tổng số tiền USD từ VND sử dụng tỷ giá
                var totalAmount = cartItems.Sum(p => (p.Quantity * p.Price) / exchangeRate).ToString("F2");
                var currency = "USD";
                var referenceOrderId = "Order" + DateTime.Now.Ticks;

                // Tạo đơn hàng trên PayPal
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
                    var orderDetails = new OrderDetails();


                    orderDetails.UserName = userEmail;
                    orderDetails.OrderCode = ordercode;
                    orderDetails.ProductId = cart.ProductId;
                    orderDetails.Price = cart.Price;
                    orderDetails.Quantity = cart.Quantity;
                    orderDetails.FullName = model.FullName;
                    orderDetails.Phone = model.Phone;
                    orderDetails.Address = model.Address;
                    orderDetails.PaymentId = 2;


                    //update product quantity
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);
                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();
                }
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
