using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_NET104.Tests
{
    [TestFixture]
    public class FoodServiceTests
    {
        private AppDbContext _context;
        private FoodService _foodService;

        [SetUp]
        public void Setup()
        {
            // ✅ Dùng chung một database để tránh mất dữ liệu giữa các test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            _context = new AppDbContext(options);
            _foodService = new FoodService(_context);

            // ✅ Seed dữ liệu đầy đủ (nếu chưa có)
            // Seed Category if not exists
            if (!_context.Categories.Any())
            {
                _context.Categories.AddRange(
                    new Category { Id = 1, Name = "Burger" },
                    new Category { Id = 2, Name = "Pizza" }
                );
                _context.SaveChanges();
            }

            // Seed FoodItems with all required fields
            if (!_context.FoodItems.Any())
            {
                _context.FoodItems.AddRange(
                    new FoodItem
                    {
                        Id = 1,
                        Name = "Burger",
                        Price = 5.99m,
                        Description = "Classic beef burger",
                        ImagePath = "https://via.placeholder.com/150",
                        IsAvailable = true,
                        CategoryId = 1,
                        Quantity = 10
                    },
                    new FoodItem
                    {
                        Id = 2,
                        Name = "Pizza",
                        Price = 8.99m,
                        Description = "Cheese pizza",
                        ImagePath = "https://via.placeholder.com/150",
                        IsAvailable = true,
                        CategoryId = 2,
                        Quantity = 10
                    }
                );
                _context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
           
            _context.Dispose();
        }

        // 🔹 1. Test lấy món ăn hợp lệ
        [Test]
        public async Task GetFoodById_ValidId_ReturnsFood()
        {
            var food = await _foodService.GetByIdAsync(2);
            Assert.IsNotNull(food);
            Assert.AreEqual("Pizza", food.Name);
        }
        // 🔹 2. Test lấy món ăn không tồn tại
        [Test]
        public async Task GetFoodById_InvalidId_ReturnsNull()
        {
            var food = await _foodService.GetByIdAsync(999);
            Assert.IsNull(food);
        }

        // 🔹 3. Test tạo món ăn mới hợp lệ
        [Test]
        public async Task CreateFood_ValidData_AddsFoodToDatabase()
        {
            var newFood = new FoodItem { Name = "Pasta", Price = 7.99m };

            await _foodService.CreateAsync(newFood);

            var createdFood = _context.FoodItems.FirstOrDefault(f => f.Name == "Pasta");
            Assert.IsNotNull(createdFood);
            Assert.AreEqual(7.99m, createdFood.Price);
            Assert.AreEqual("https://via.placeholder.com/150", createdFood.ImagePath); // kiểm tra ảnh mặc định
        }

        // 🔹 4. Test tự động điền mô tả mặc định
        [Test]
        public async Task CreateFood_MissingDescription_ShouldAutoFillDefaults()
        {
            var newFood = new FoodItem { Name = "Soup", Price = 3.5m, ImagePath = null, Description = null };

            await _foodService.CreateAsync(newFood);

            var created = _context.FoodItems.FirstOrDefault(f => f.Name == "Soup");
            Assert.IsNotNull(created);
            Assert.AreEqual("Chưa có mô tả", created.Description);
            Assert.IsTrue(created.IsAvailable);
        }

        // 🔹 5. Test xoá món ăn hợp lệ
        [Test]
        public async Task DeleteFood_ExistingItem_RemovesFromDatabase()
        {
            await _foodService.DeleteAsync(1);
            var deleted = await _foodService.GetByIdAsync(1);
            Assert.IsNull(deleted);
        }

        // 🔹 6. Test xoá món ăn không tồn tại
        [Test]
        public async Task DeleteFood_NonExistingItem_DoesNothing()
        {
            await _foodService.DeleteAsync(999);
            var count = _context.FoodItems.Count();
            Assert.GreaterOrEqual(count, 1); // vẫn còn ít nhất 1 item
        }

        // 🔹 7. Test tìm kiếm món ăn theo từ khoá
        [Test]
        public async Task SearchFood_ByKeyword_ReturnsMatchingItems()
        {
            var results = await _foodService.SearchAsync("Pizza", null, null, null);

            Assert.IsNotNull(results, "Search results should not be null");
            Assert.IsTrue(results.Any(), "No results found for keyword 'Pizza'");
            Assert.AreEqual("Pizza", results.First().Name);
        }

        // 🔹 8. Test cập nhật món ăn
        [Test]
        public async Task UpdateFood_ChangesAreSaved()
        {
            var food = await _foodService.GetByIdAsync(2);
            Assert.IsNotNull(food, "Food item with ID=2 not found for update test");

            food.Price = 9.99m;

            await _foodService.UpdateAsync(food);
            var updated = await _foodService.GetByIdAsync(2);

            Assert.IsNotNull(updated);
            Assert.AreEqual(9.99m, updated.Price);
        }
    }
}
