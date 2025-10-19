using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace Assignment_NET104.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        // Lưu OTP thread-safe
        private static ConcurrentDictionary<string, (string Code, DateTime Expiry)> otpStorage
            = new ConcurrentDictionary<string, (string, DateTime)>();

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState không hợp lệ:");
                foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($" - {err.ErrorMessage}");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
                Console.WriteLine("❌ Email rỗng.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                Console.WriteLine($"❌ Không tìm thấy user với email: {model.Email}");
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            Console.WriteLine($"✅ Tìm thấy user: {user.Email}, UserName={user.UserName}, Id={user.Id}");
            Console.WriteLine($"🔑 PasswordHash trong DB: {user.PasswordHash}");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Kiểm tra role
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "AdminFood");
                }
                else if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Role khác
                    return RedirectToAction("Index", "Home");
                }
            }
            if (result.IsLockedOut)
            {
                Console.WriteLine("⚠️ Tài khoản bị khóa.");
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
            }
            else if (result.IsNotAllowed)
            {
                Console.WriteLine("⚠️ User chưa được phép đăng nhập (Email chưa confirm?).");
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt.");
            }
            else
            {
                Console.WriteLine("❌ Đăng nhập thất bại: Sai mật khẩu?");
                ModelState.AddModelError("", "Đăng nhập không thành công. Vui lòng kiểm tra lại Email/Mật khẩu.");
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError("", $"External provider error: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (signInResult.Succeeded)
            {
                // Đảm bảo user đã có role Customer sau khi đăng nhập thành công
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && !await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                return LocalRedirect(returnUrl);
            }

            var emailNew = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var picture = info.Principal.FindFirstValue("picture");

            var userNew = await _userManager.FindByEmailAsync(emailNew);
            if (userNew == null)
            {
                userNew = new ApplicationUser
                {
                    UserName = emailNew,
                    Email = emailNew,
                    FullName = name ?? "Người dùng mới",
                    Address = "Chưa cập nhật",
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(userNew);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return RedirectToAction(nameof(Login));
                }
            }

            // Đảm bảo role Customer tồn tại và được gán cho user
            if (!await _roleManager.RoleExistsAsync("Customer"))
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            if (!await _userManager.IsInRoleAsync(userNew, "Customer"))
                await _userManager.AddToRoleAsync(userNew, "Customer");

            await _userManager.AddLoginAsync(userNew, info);
            await _signInManager.SignInAsync(userNew, false);

            return LocalRedirect(returnUrl);
        }

        // ================= REGISTER + OTP =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Email không hợp lệ" });

            var otpCode = new Random().Next(100000, 999999).ToString();
            otpStorage[email] = (otpCode, DateTime.Now.AddMinutes(5));

            await _emailSender.SendEmailAsync(email, "Mã OTP xác thực",
                $"Mã OTP của bạn là: <b>{otpCode}</b>. Mã này sẽ hết hạn sau 5 phút.");

            return Json(new { success = true, message = "OTP đã được gửi qua email" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Invalid:");
                foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($" - {err.ErrorMessage}");
                return View(model);
            }

            // Kiểm tra OTP
            if (!otpStorage.TryGetValue(model.Email, out var storedOtp))
            {
                ModelState.AddModelError("", "OTP không tồn tại. Vui lòng gửi lại OTP.");
                return View(model);
            }

            if (storedOtp.Code != model.OtpCode || storedOtp.Expiry < DateTime.Now)
            {
                ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            // Xóa OTP
            otpStorage.TryRemove(model.Email, out _);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                Address = model.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    Console.WriteLine($"Identity Error: {error.Description}");
                }
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        Console.WriteLine($"Role Error: {error.Description}");
                    }
                    return View(model);
                }
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction("Login", "Account");
        }


        // ================= LOGOUT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
