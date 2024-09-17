using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Repository;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Sliders.OrderByDescending(p => p.Id).ToListAsync());

        }
        public async Task<IActionResult> Edit(long Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
            return View(slider);
        }
        #region Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSlider = await _dataContext.Sliders.FindAsync(slider.Id);

                    if (existingSlider == null)
                    {
                        TempData["error"] = "Slider không tồn tại.";
                        return RedirectToAction("Index");
                    }

                    List<string> SliderFileNames = new List<string>();
                  
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");

                    // Xử lý ảnh 
                    if (slider.UploadSlider != null && slider.UploadSlider.Length > 0)
                    {
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        foreach (var file in slider.UploadSlider)
                        {
                            string extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                            string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "." + extension;
                            string filePath = Path.Combine(uploadsDir, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            SliderFileNames.Add(fileName);
                        }

                        existingSlider.Name = string.Join(",", SliderFileNames);
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _dataContext.Update(existingSlider);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật slider thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi cập nhật slider {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(slider);
                }
            }
            else
            {
                TempData["error"] = "Model không hợp lệ.";
                return View(slider);
            }
        }
        #endregion
    }
}
