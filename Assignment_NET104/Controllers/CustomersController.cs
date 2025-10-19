using Assignment_NET104.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_NET104.Controllers
{
    public class CustomersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Lấy user đang đăng nhập
            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user); // Truyền ApplicationUser vào View
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ApplicationUser model)
        {
            if (!ModelState.IsValid)   // ✅ kiểm tra dữ liệu hợp lệ
            {
                return View(model);   // trả lại form kèm lỗi validation
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Cập nhật dữ liệu
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Message"] = "Cập nhật thành công!";
                return RedirectToAction("Profile");
            }

            // Nếu có lỗi từ Identity (VD: format số điện thoại không đúng chuẩn)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model); // Trả lại form cùng lỗi
        }

        public IActionResult OrderHistory()
        {
            return View();
        }

        public IActionResult TrackOrder()
        {
            return View();
        }
    }
}
