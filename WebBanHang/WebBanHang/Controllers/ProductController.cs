using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;
using WebBanHang.Repository;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        // Chi tiết sản phẩm
        public async Task<IActionResult> Details(long Id)
        {
            // Kiểm tra Id có hợp lệ không
            if (Id == 0)
            {
                return RedirectToAction("Index");
            }

            // Tìm sản phẩm theo Id
            var productById = await _dataContext.Products
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (productById == null)
            {
                // Sản phẩm không tồn tại
                return NotFound();
            }

            // Lấy sản phẩm liên quan
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productById
            };

            return View(viewModel);
        }

        // Xử lý bình luận sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo đối tượng RatingModel từ thông tin nhận được
                    var ratingEntity = new RatingModel
                    {
                        ProductId = rating.ProductId,
                        Name = rating.Name,
                        Email = rating.Email,
                        Comment = rating.Comment,
                        Star = rating.Star
                    };

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Ratings.Add(ratingEntity);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Thêm đánh giá thành công";

                    return Redirect(Request.Headers["Referer"].ToString());
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm đánh giá: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return RedirectToAction("Details", new { id = rating.ProductId });
                }
            }
            else
            {
                TempData["error"] = "Thông tin nhập vào không hợp lệ. Vui lòng kiểm tra lại.";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }

                string errorMessage = string.Join("\n", errors);
                TempData["validationErrors"] = errorMessage;

                return RedirectToAction("Details", new { id = rating.ProductId });
            }
        }
    }
}
