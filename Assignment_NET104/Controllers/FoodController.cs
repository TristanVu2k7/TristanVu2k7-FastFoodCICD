using Assignment_NET104.Data;
using Assignment_NET104.Models; // <-- Add this using directive if OrderDetail is in Models namespace
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment_NET104.Controllers
{
    [Authorize(Roles = "Admin,User, Customer")]
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly AppDbContext _context;

        public FoodController(IFoodService foodService, AppDbContext context)
        {
            _foodService = foodService;
            _context = context;
        }

        // Danh sách món ăn cho khách
        public async Task<IActionResult> Index(string search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var foods = await _foodService.SearchAsync(search, categoryId, minPrice, maxPrice);
            return View(foods);
        }

        // Xem chi tiết món ăn - ai cũng truy cập được
        public async Task<IActionResult> Details(int id)
        {
            var food = await _foodService.GetByIdAsync(id);
            if (food == null)
            {
                return NotFound();
            }
            return View(food);
        }

        // Nếu dùng Razor Pages:
        public async Task<IActionResult> OnPostPlaceOrderAsync(int id)
        {
            // Lấy thông tin món ăn và người dùng
            var foodItem = await _context.FoodItems.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Processing, // <-- Use enum value instead of string
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        FoodItemId = foodItem.Id,
                        Quantity = 1,
                        Price = foodItem.Price
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Chuyển hướng về lịch sử đơn hàng
            return RedirectToPage("/Order/History");
        }
    }
}
