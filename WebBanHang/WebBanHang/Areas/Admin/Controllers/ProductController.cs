using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
using WebBanHang.Models;
using WebBanHang.Repository;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Master")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int pg =1)
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());

        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    product.Slug = product.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Sản phẩm đã có trong database");
                        return View(product);
                    }

                    // Xử lý tệp hình ảnh
                    if (product.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        //string imageName = Guid.NewGuid().ToString() + "_" +product.ImageUpload.FileName;
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        string extension = Path.GetExtension(product.ImageUpload.FileName).TrimStart('.').ToLower();
                        string fileName = Guid.NewGuid().ToString() + "." + extension;
                        string filePath = Path.Combine(uploadsDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(stream);
                        }

                        product.Image = fileName;
                    }

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Add(product);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Thêm sản phẩm thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm sản phẩm: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(product);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            var existed_product = _dataContext.Products.Find(product.Id);
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    product.Slug = product.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products
                        .FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Sản phẩm đã có trong database");
                        return View(product);
                    }

                    // Xử lý tệp hình ảnh
                    if (product.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        //string imageName = Guid.NewGuid().ToString() + "_" +product.ImageUpload.FileName;
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        string extension = Path.GetExtension(product.ImageUpload.FileName).TrimStart('.').ToLower();
                        string fileName = Guid.NewGuid().ToString() + "." + extension;
                        string filePath = Path.Combine(uploadsDir, fileName);
                        //Delete old picture
                        string oldfileImage = Path.Combine(uploadsDir, existed_product.Image);
                        try
                        {
                            if (System.IO.File.Exists(oldfileImage))
                            {
                                System.IO.File.Delete(oldfileImage);
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "An error occurred while deleting the product image.");
                        }

                        //Create new picture
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(stream);
                        }

                        existed_product.Image = fileName;
                    }
                    //Update các thuộc tính khác
                    existed_product.Name = product.Name;
                    existed_product.Description = product.Description;
                    existed_product.Price = product.Price;
                    existed_product.CategoryId = product.CategoryId;
                    existed_product.BrandId = product.BrandId;

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Update(existed_product);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm sản phẩm: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(product);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(product);
            }
        }

        public async Task<IActionResult> Delete(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if(!string.Equals(product.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldfileImage = Path.Combine(uploadsDir, product.Image);
                try
                {
                    if (System.IO.File.Exists(oldfileImage))
                    {
                        System.IO.File.Delete(oldfileImage);
                    }
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the product image.");
                }
               
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Sản phẩm đã xoá";
            return RedirectToAction("Index");
        }
    }
}
