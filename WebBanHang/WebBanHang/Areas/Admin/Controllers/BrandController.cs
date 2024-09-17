using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Repository;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Master")]

    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        #region Create - Edit - Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            //return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
            List<BrandModel> brand = _dataContext.Brands.ToList();

            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int resCount = brand.Count();
            var pager = new Paginate(resCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = brand.Skip(recSkip).Take(pageSize).ToList();
            ViewBag.Pager = pager;
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(long Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }
        #endregion

        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Thương hiệu đã có trong database");
                        return View(brand);
                    }

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Add(brand);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Thêm thương hiệu thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm thương hiệu: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(brand);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(brand);
            }
        }
        #endregion

        #region Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Thương hiệu đã có trong database");
                        return View(brand);
                    }

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Update(brand);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Sửa thương hiệu thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi sửa thương hiệu: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(brand);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(brand);
            }
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(long Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Thương hiệu đã xoá";
            return RedirectToAction("Index");
        }
        #endregion
    }
}
