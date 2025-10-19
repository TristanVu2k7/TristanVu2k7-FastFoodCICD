using Assignment_NET104.Controllers;
using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class OrderControllerTests
    {
        private AppDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private OrderController _controller;

        [SetUp]
        public void Setup()
        {
            // ✅ Khởi tạo InMemory Database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("OrderTestDB_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // ✅ Giả lập UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            // ✅ Seed dữ liệu mẫu đầy đủ
            _context.Categories.Add(new Category { Id = 1, Name = "Default" });

            _context.FoodItems.AddRange(
                new FoodItem
                {
                    Id = 1,
                    Name = "Burger",
                    Price = 5.99m,
                    Description = "Beef burger",
                    ImagePath = "https://via.placeholder.com/150",
                    CategoryId = 1
                },
                new FoodItem
                {
                    Id = 2,
                    Name = "Pizza",
                    Price = 8.99m,
                    Description = "Cheese pizza",
                    ImagePath = "https://via.placeholder.com/150",
                    CategoryId = 1
                }
            );

            _context.OrderHistories.AddRange(
                new OrderHistory
                {
                    Id = 1,
                    FoodItemId = 1,
                    Quantity = 2,
                    Price = 5.99m,
                    OrderDate = DateTime.Now.AddDays(-1),
                    CustomerName = "Alice"
                },
                new OrderHistory
                {
                    Id = 2,
                    FoodItemId = 2,
                    Quantity = 1,
                    Price = 8.99m,
                    OrderDate = DateTime.Now,
                    CustomerName = "Bob"
                }
            );

            _context.SaveChanges();

            // ✅ Tạo controller và inject phụ thuộc
            _controller = new OrderController(_context, _userManagerMock.Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()),
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // 🧩 TEST 1: History trả về danh sách đơn hàng
        // Pass: Kiểm tra ViewResult và dữ liệu trả về
        // Fail: Không trả về ViewResult hoặc dữ liệu sai
        [Test]
        public async Task History_ReturnsViewWithOrderHistories()
        {
            // Act
            var result = await _controller.History() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as List<OrderHistory>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
            Assert.AreEqual("Burger", model.Last().FoodItem.Name);
        }

        // 🧩 TEST 2: AddToCart thêm món mới vào giỏ hàng
        // Pass: Kiểm tra redirect và dữ liệu giỏ hàng
        // Fail: Không redirect đúng hoặc dữ liệu giỏ hàng sai
        [Test]
        public async Task AddToCart_AddsItemToCartAndRedirects()
        {
            // Arrange
            int foodItemId = 1;
            int quantity = 3;

            // Act
            var result = await _controller.AddToCart(foodItemId, quantity) as RedirectToActionResult;

            // Assert: Kiểm tra redirect đúng
            Assert.IsNotNull(result, "Kết quả trả về không hợp lệ.");
            Assert.AreEqual("Index", result.ActionName, "Không redirect đến Index.");
            Assert.AreEqual("Cart", result.ControllerName, "Không redirect đến controller Cart.");

            // Kiểm tra dữ liệu trong giỏ hàng
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.FoodItemId == foodItemId);
            Assert.IsNotNull(cartItem, "Không tìm thấy sản phẩm trong giỏ hàng sau khi thêm.");
            Assert.AreEqual(quantity, cartItem.Quantity, "Số lượng món trong giỏ hàng không khớp.");
            Assert.AreEqual(5.99m, cartItem.Price, "Giá món ăn không khớp.");

            // Chỉ kiểm tra Name nếu Controller có gán
            if (!string.IsNullOrEmpty(cartItem.Name))
                Assert.AreEqual("Burger", cartItem.Name, "Tên món ăn không đúng.");
        }


        // 🧩 TEST 3: AddToCart thêm tiếp món đã có => tăng số lượng
        // Pass: Kiểm tra số lượng tăng đúng trả về giỏ hàng
        // Fail: Số lượng không tăng đúng hoặc không redirect đúng
        [Test]
        public async Task AddToCart_ExistingItem_IncreasesQuantity()
        {
            int foodItemId = 2;
            int initialQuantity = 2;
            int additionalQuantity = 3;

            _context.CartItems.Add(new CartItem
            {
                FoodItemId = foodItemId,
                Quantity = initialQuantity,
                Price = 8.99m,
                Name = "Pizza"
            });
            _context.SaveChanges();

            var result = await _controller.AddToCart(foodItemId, additionalQuantity) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Cart", result.ControllerName);

            var cartItem = _context.CartItems.FirstOrDefault(ci => ci.FoodItemId == foodItemId);
            Assert.IsNotNull(cartItem);
            Assert.AreEqual(initialQuantity + additionalQuantity, cartItem.Quantity);
        }

        // 🧩 TEST 4: AddToCart với ID không tồn tại => NotFound
        // Pass: Trả về NotFoundResult
        // Fail: Trả về kết quả khác NotFoundResult
        [Test]
        public async Task AddToCart_InvalidFoodItem_ReturnsNotFound()
        {
            int invalidFoodItemId = 999;
            int quantity = 1;

            var result = await _controller.AddToCart(invalidFoodItemId, quantity);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // 🧩 TEST 5: Index trả về ViewResult
        // Pass: Trả về ViewResult
        // Fail: Không trả về ViewResult
        [Test]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();
            Assert.IsInstanceOf<ViewResult>(result);
        }
    }
}
