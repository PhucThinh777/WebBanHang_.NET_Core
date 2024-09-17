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
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var contact = _dataContext.Contacts.ToList();
            return View(contact);
        }

        public async Task<IActionResult> Edit(long Id)
        {
            ContactModel contact = await _dataContext.Contacts.FindAsync(Id);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var existed_contact = _dataContext.Contacts.Find(contact.Id);
            if (ModelState.IsValid)
            {
                try
                {
                    
                    // Xử lý tệp hình ảnh
                    if (contact.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logos");
                        
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        string extension = Path.GetExtension(contact.ImageUpload.FileName).TrimStart('.').ToLower();
                        string fileName = Guid.NewGuid().ToString() + "." + extension;
                        string filePath = Path.Combine(uploadsDir, fileName);
                        //Delete old picture
                        string oldfileImage = Path.Combine(uploadsDir, existed_contact.Logo);
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
                            await contact.ImageUpload.CopyToAsync(stream);
                        }

                        existed_contact.Logo = fileName;
                    }
                    //Update các thuộc tính khác
                    existed_contact.Name = contact.Name;
                    existed_contact.Description = contact.Description;
                    existed_contact.Map = contact.Map;
                    existed_contact.Phone = contact.Phone;

                    // Lưu đối tượng vào cơ sở dữ liệu
                    _dataContext.Update(existed_contact);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật thông tin liên hệ thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đã xảy ra lỗi khi thêm sản phẩm: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["error"] += $" Inner exception: {ex.InnerException.Message}";
                    }

                    return View(contact);
                }
            }
            else
            {
                TempData["error"] = "Model đang thiếu";
                return View(contact);
            }
        }
    }
}
