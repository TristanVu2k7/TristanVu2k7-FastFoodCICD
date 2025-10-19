using Assignment_NET104.Controllers;
using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFood.IntegrationTest
{
    [TestFixture]
    public class CartControllerTests
    {
        private AppDbContext _context;
        private CartController _controller;
        private IFoodService _foodService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("CartTestDB_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Seed dữ liệu mẫu
            _context.Categories.Add(new Category { Id = 1, Name = "Default" });
            _context.FoodItems.AddRange(
                new FoodItem
                {
                    Id = 1,
                    Name = "Burger",
                    Price = 5.99m,
                    Description = "Tasty burger",
                    ImagePath = "http://example.com/burger.jpg",
                    CategoryId = 1
                },
                new FoodItem
                {
                    Id = 2,
                    Name = "Pizza",
                    Price = 8.99m,
                    Description = "Cheesy pizza",
                    ImagePath = "http://example.com/pizza.jpg",
                    CategoryId = 1
                }
            );
            _context.SaveChanges();

            _foodService = new FoodService(_context);
            _controller = new CartController(_foodService, _context);

            // ✅ Thêm TempData và HttpContext để tránh NullReferenceException
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ✅ Test: Index hiển thị đúng giỏ hàng
        // Pass : Nếu phương thức Index trả về View với danh sách CartItems và tổng tiền đúng.
        // Fail: Nếu phương thức Index không trả về View đúng hoặc tổng tiền sai.
        [Test]
        public async Task Index_ReturnsViewWithCartItems()
        {
            _context.CartItems.Add(new CartItem { Id = 1, FoodItemId = 1, Quantity = 2, Price = 5.99m, Name = "Burger" });
            _context.SaveChanges();

            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as List<CartItem>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(result.ViewData["Total"], Is.EqualTo(11.98m));
        }

        // ✅ Test: Thêm sản phẩm vào giỏ hàng
        // Pass: Nếu sản phẩm hợp lệ được thêm vào giỏ và chuyển hướng đúng.
        // Fail: Nếu sản phẩm không được thêm hoặc chuyển hướng sai.
        [Test]
        public async Task Add_ValidFoodItem_AddsToCartAndRedirects()
        {
            var result = await _controller.Add(1) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.CartItems.Count(), Is.EqualTo(1));
            Assert.That(_context.CartItems.First().Name, Is.EqualTo("Burger"));
        }

        // ✅ Test: Xóa sản phẩm khỏi giỏ hàng
        // Pass: Nếu sản phẩm tồn tại được xóa và chuyển hướng đúng.
        // Fail: Nếu sản phẩm không được xóa hoặc chuyển hướng sai.
        [Test]
        public async Task Remove_ExistingItem_RemovesItemAndRedirects()
        {
            _context.CartItems.Add(new CartItem { Id = 1, FoodItemId = 1, Quantity = 1, Price = 5.99m, Name = "Burger" });
            _context.SaveChanges();

            var result = await _controller.Remove(1) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.CartItems.Count(), Is.EqualTo(0));
        }

        // ✅ Test: Checkout hiển thị đúng tổng tiền
        // Pass: Nếu giỏ hàng rỗng trả về View với tổng tiền 0.
        // Fail: Nếu giỏ hàng rỗng không trả về View đúng hoặc tổng tiền sai.
        [Test]
        public async Task Checkout_WithItems_ReturnsViewWithTotal()
        {
            _context.CartItems.Add(new CartItem { Id = 1, FoodItemId = 1, Quantity = 1, Price = 5.99m, Name = "Burger" });
            _context.SaveChanges();

            var result = await _controller.Checkout() as ViewResult;

            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.Null.Or.EqualTo("Checkout"));
            Assert.That(result.ViewData["Total"], Is.EqualTo(5.99m));
        }

        // ✅ Test: ConfirmCheckout tạo OrderHistory và xóa giỏ hàng
        // Pass: Nếu đơn hàng được tạo và giỏ hàng bị xóa sau khi xác nhận thanh toán.
        // Fail: Nếu đơn hàng không được tạo hoặc giỏ hàng không bị xóa.
        [Test]
        public async Task ConfirmCheckout_CreatesOrderHistoryAndClearsCart()
        {
            _context.CartItems.Add(new CartItem
            {
                Id = 1,
                FoodItemId = 1,
                Quantity = 2,
                Price = 5.99m,
                Name = "Burger"
            });
            _context.SaveChanges();

            var result = await _controller.ConfirmCheckout() as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("CompleteCheckout"));
            Assert.That(_context.CartItems.Count(), Is.EqualTo(0));

            var orderHistory = _context.OrderHistories.FirstOrDefault();
            Assert.IsNotNull(orderHistory);
            Assert.That(orderHistory.FoodItemId, Is.EqualTo(1));
            Assert.That(orderHistory.Quantity, Is.EqualTo(2));
        }
    }
}
