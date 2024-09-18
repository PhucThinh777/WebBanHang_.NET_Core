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
                    var receiver = user.Email;
                    var subject = "Tạo tài khoản thành công";
                    var message = "Bạn vừa tạo tài khoản thành công trên website PT Shop.";

                    await _emailSender.SendEmailAsync(receiver, subject, message);
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
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
        #endregion

        #region Quên mật khẩu (ForgotPassword)
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ForgotPasswordError");
            }
            else
            {
                // Tạo token để reset mật khẩu
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, protocol: HttpContext.Request.Scheme);

                // Gửi email reset mật khẩu
                var subject = "Đặt lại mật khẩu";
                var message = $"Nhấp vào <a href='{callbackUrl}'>đây</a> để đặt lại mật khẩu.";
                await _emailSender.SendEmailAsync(model.Email, subject, message);
                return RedirectToAction("ForgotPasswordConfirmation");
            }
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        public IActionResult ForgotPasswordError()
        {
            return View();
        }
        #endregion

        #region Đặt lại mật khẩu (ResetPassword)
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return View("Error");
            }
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Nếu người dùng không tồn tại, không tiết lộ thông tin
                return RedirectToAction("ResetPasswordConfirmation");
            }

            // Reset mật khẩu
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion
    }
}
