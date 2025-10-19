using Assignment_NET104.Controllers;
using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Hosting;
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
        private Mock<IWebHostEnvironment> _envMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("CartTestDB_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // ✅ Giả lập môi trường chạy test
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.EnvironmentName).Returns("Test");

            // ✅ Seed dữ liệu mẫu
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
            _controller = new CartController(_foodService, _context, _envMock.Object);

            // ✅ Khởi tạo TempData và HttpContext để tránh lỗi null
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

        // ✅ Test 1: Hiển thị giỏ hàng
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

        // ✅ Test 2: Thêm sản phẩm vào giỏ hàng
        [Test]
        public async Task Add_ValidFoodItem_AddsToCartAndRedirects()
        {
            var result = await _controller.Add(1) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.CartItems.Count(), Is.EqualTo(1));
            Assert.That(_context.CartItems.First().Name, Is.EqualTo("Burger"));
        }

        // ✅ Test 3: Xóa sản phẩm khỏi giỏ hàng
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

        // ✅ Test 4: Hiển thị trang Checkout
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

        // ✅ Test 5: Xác nhận thanh toán thành công
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
            Assert.That(orderHistory.CustomerName, Is.EqualTo("TestUser"));
        }
    }
}
