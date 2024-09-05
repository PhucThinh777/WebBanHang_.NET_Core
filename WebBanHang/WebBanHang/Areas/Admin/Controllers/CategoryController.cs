using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Repository;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Master")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        #region Create - Edit -Index

        public async Task<IActionResult> Index(int pg =1)
        {
            //return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
            List<CategoryModel> category = _dataContext.Categories.ToList();

            const int pageSize = 10;

            if(pg < 1)
            {
                pg = 1;
            }
            int resCount = category.Count();
            var pager = new Paginate(resCount, pg , pageSize);
            int recSkip = (pg -1) * pageSize;
            var data = category.Skip(recSkip).Take(pageSize).ToList();
            ViewBag.Pager = pager;
            return View(data);
        }

        public IActionResult Create() 
        {
            return View();
        }

        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }
        #endregion

        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    category.Slug = category.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Danh mục đã có trong database");
                        return View(category);
                    }

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Add(category);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Thêm danh mục thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm danh mục: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(category);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(category);
            }
        }
        #endregion

        #region Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán giá trị cho Slug
                    category.Slug = category.Name.Replace(" ", "-").ToLower();

                    // Kiểm tra trùng lặp Slug
                    var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("", "Danh mục đã có trong database");
                        return View(category);
                    }

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Update(category);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Sửa danh mục thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi sửa danh mục: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(category);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(category);
            }
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Danh mục đã xoá";
            return RedirectToAction("Index");
        }
        #endregion
    }
}
