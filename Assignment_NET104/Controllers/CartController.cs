using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_NET104.Controllers
{
    public class CartController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CartController(IFoodService foodService, AppDbContext context, IWebHostEnvironment env)
        {
            _foodService = foodService;
            _context = context;
            _env = env;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await _context.CartItems.Include(c => c.FoodItem).ToListAsync();
            ViewBag.Total = cart.Sum(c => c.Price * c.Quantity);
            return View(cart);
        }

        // GET: /Cart/Add/1
        [HttpGet("Cart/Add/{foodItemId}")]
        public async Task<IActionResult> Add(int foodItemId)
        {
            var food = await _context.FoodItems.FindAsync(foodItemId);
            if (food == null) return NotFound();

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.FoodItemId == foodItemId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    FoodItemId = foodItemId,
                    Quantity = 1,
                    Price = food.Price,
                    Name = food.Name
                });
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"Đã thêm {food.Name} vào giỏ!";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cart = await _context.CartItems.Include(c => c.FoodItem).ToListAsync();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            ViewBag.Total = cart.Sum(c => c.Price * c.Quantity);
            return View(cart); // Checkout.cshtml
        }

        // POST: /Cart/ConfirmCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout()
        {
            var cart = await _context.CartItems.Include(c => c.FoodItem).ToListAsync();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            // ✅ Nếu là môi trường test hoặc dev => bỏ qua OTP/xác thực
            string customerName;
            if (_env.EnvironmentName == "Test" || _env.EnvironmentName == "Development")
            {
                customerName = "TestUser"; // gán người dùng giả
            }
            else
            {
                customerName = User.Identity?.Name ?? "Khách";
            }

            // Lưu đơn hàng vào OrderHistory
            foreach (var item in cart)
            {
                _context.OrderHistories.Add(new OrderHistory
                {
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderDate = DateTime.Now,
                    CustomerName = customerName
                });
            }

            // Xóa giỏ hàng sau khi thanh toán
            _context.CartItems.RemoveRange(cart);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Thanh toán thành công! (Người đặt: {customerName})";
            return RedirectToAction("CompleteCheckout");
        }

        // GET: /Cart/CompleteCheckout
        public IActionResult CompleteCheckout()
        {
            return View(); // CompleteCheckout.cshtml
        }
    }
}
