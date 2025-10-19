using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace Assignment_NET104.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFoodController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AdminFoodController> _logger;

        public AdminFoodController(
            IFoodService foodService,
            ICategoryService categoryService,
            ILogger<AdminFoodController> logger)
        {
            _foodService = foodService;
            _categoryService = categoryService;
            _logger = logger;
        }

        // Danh sách món ăn
        public async Task<IActionResult> Index()
        {
            try
            {
                var foods = await _foodService.GetAllAsync();
                return View(foods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách món ăn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu";
                return View(new List<FoodItem>());
            }
        }

        // Form tạo mới
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View(new FoodItem());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem food)
        {
            try
            {
                // Clear ModelState trước khi validate
                ModelState.Remove("Category"); // Nếu có navigation property

                if (food.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return View(food);
                }

                // Xử lý lưu dữ liệu
                await _foodService.CreateAsync(food);
                TempData["SuccessMessage"] = "Thêm món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm món ăn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm món ăn";
                await LoadCategories();
                return View(food);
            }
        }
        // Form chỉnh sửa
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var food = await _foodService.GetByIdAsync(id);
                if (food == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món ăn";
                    return RedirectToAction(nameof(Index));
                }
                await LoadCategories();
                return View(food);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải món ăn ID {id} để chỉnh sửa");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem food)
        {
            try
            {
                if (id != food.Id)
                {
                    TempData["ErrorMessage"] = "ID không khớp";
                    return RedirectToAction(nameof(Index));
                }

                // Xử lý validate cho CategoryId
                if (food.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục hợp lệ");
                }

                // Nếu có navigation property Category thì xóa khỏi ModelState để tránh lỗi required
                ModelState.Remove("Category");

                // Validate URL hình ảnh
                if (string.IsNullOrWhiteSpace(food.ImagePath))
                {
                    ModelState.AddModelError("ImagePath", "Vui lòng nhập URL hình ảnh");
                }
                else if (!Uri.IsWellFormedUriString(food.ImagePath, UriKind.Absolute))
                {
                    ModelState.AddModelError("ImagePath", "URL không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return View(food);
                }

                await _foodService.UpdateAsync(food);
                TempData["SuccessMessage"] = "Cập nhật món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật món ăn ID {id}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật";
                await LoadCategories();
                return View(food);
            }
        }

        // Xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var food = await _foodService.GetByIdAsync(id);
                if (food == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món ăn";
                    return RedirectToAction(nameof(Index));
                }
                return View(food);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải món ăn ID {id} để xóa");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var food = await _foodService.GetByIdAsync(id);
            if (food == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món ăn";
                return RedirectToAction(nameof(Index));
            }
            return View(food); // Trả về view DeleteConfirmed.cshtml
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedPost(int id)
        {
            var food = await _foodService.GetByIdAsync(id);
            if (food == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món ăn";
                return RedirectToAction(nameof(Index));
            }

            await _foodService.DeleteAsync(id);
            // Chuyển hướng về Index thay vì trả về view DeleteConfirmed để tránh lỗi truy cập trực tiếp
            TempData["SuccessMessage"] = $"Đã xóa món ăn '{food.Name}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }
    }
}