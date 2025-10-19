using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment_NET104.Controllers
{
    [Authorize(Roles = "Customer, User")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrderController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            var orders = await _context.OrderHistories
                .Include(o => o.FoodItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // View model List<OrderHistory>
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int foodItemId, int quantity)
        {
            var foodItem = await _context.FoodItems.FindAsync(foodItemId);
            if (foodItem == null) return NotFound();

            // tìm xem đã có trong giỏ chưa
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.FoodItemId == foodItemId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    FoodItemId = foodItem.Id,
                    Quantity = quantity,
                    Price = foodItem.Price,
                     Name = foodItem.Name
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Cart");
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
