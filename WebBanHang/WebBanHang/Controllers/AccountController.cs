using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebBanHang.Areas.Admin.Repository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;

namespace WebBanHang.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(IEmailSender emailSender, SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        #region Login
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM, AppUserModel appUserModel)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
                    .PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                   // Gửi Email
                   //var receiver = ""; 
                   //var subject = "Đăng nhập";
                   //var message = "Bạn vừa đăng nhập thành công.";

                   //await _emailSender.SendEmailAsync(receiver, subject, message);
                        return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Username hoặc Password bị sai");
            }
            return View(loginVM);
        }
        #endregion

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công";
                    return Redirect("/account/login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout(string returnUrl ="/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
        #endregion
    }
}
