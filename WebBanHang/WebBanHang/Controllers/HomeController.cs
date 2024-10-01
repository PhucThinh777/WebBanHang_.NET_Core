using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;
using WebBanHang.Repository;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();
            var sliders = _dataContext.Sliders.ToList();
            var contacts = _dataContext.Contacts.ToList();
            ViewBag.Sliders = sliders;
            ViewBag.Contacts = contacts;

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(long Id, WishlistModel wishListModel)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishListProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };
            
            _dataContext.Add(wishListProduct);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Thêm vào WishList thành công"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi cập nhật");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCompare(long Id, CompareModel compareModel)
        {
            var user = await _userManager.GetUserAsync(User);

            var compareProduct = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Add(compareProduct);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Thêm vào Compare thành công"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi cập nhật");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if(statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            } 
        }

        public async Task<IActionResult> Contact()
        {
			var contact = await _dataContext.Contacts.FirstAsync();
			return View(contact);
		}

        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new CompareViewModel
                                         {
                                             ProductName = p.Name,
                                             Description = p.Description,
                                             Price = p.Price,
                                             Image = p.Image,
                                             CompareId = c.Id
                                         })
                                .ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                         join p in _dataContext.Products on w.ProductId equals p.Id
                                         join u in _dataContext.Users on w.UserId equals u.Id
                                          select new WishlistViewModel
                                          {
                                              ProductName = p.Name,
                                              Description = p.Description,
                                              Price = p.Price,
                                              Image = p.Image,
                                              WishlistId = w.Id
                                          })
                                .ToListAsync();
            return View(wishlist_product);
        }

        public async Task<IActionResult> DeleteCompare(long id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(id);

            if (compare == null)
            {
                return NotFound();
            }

            _dataContext.Compares.Remove(compare);
            TempData["success"] = "Xoá thành công";
            await _dataContext.SaveChangesAsync();

            return RedirectToAction("Compare","Home");
        }

        public async Task<IActionResult> DeleteWishlist(long id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(id);

            if (wishlist == null)
            {
                return NotFound();
            }

            _dataContext.Wishlists.Remove(wishlist);
            TempData["success"] = "Xoá thành công";
            await _dataContext.SaveChangesAsync();

            return RedirectToAction("Wishlist","Home");
        }
    }
}
