using NUnit.Framework;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Assignment_NET104.Data;
using Microsoft.EntityFrameworkCore;
namespace FastFood.UnitTests;

public class CategoryServiceTest
{
    private AppDbContext _context;
    private CategoryService _categoryService;
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;
        _context = new AppDbContext(options);
        _categoryService = new CategoryService(_context);
        if (!_context.Categories.Any())
        {
            _context.Categories.AddRange(
                new Category { Id = 1, Name = "Burger" },
                new Category { Id = 2, Name = "Pizza" }
            );
            _context.SaveChanges();
        }
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    [Test]
    //Lấy tất cả danh mục
    // Nếu không có lỗi thì trả về tất cả danh mục
    // Nếu có lỗi thì trả về null
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        Assert.AreEqual(2, categories.Count());
    }
    //Lấy danh mục theo Id
    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        var category = await _categoryService.GetByIdAsync(1);
        Assert.IsNotNull(category);
        Assert.AreEqual("Burger", category.Name);
    }
    //Lấy danh mục theo Id không tồn tại
    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var category = await _categoryService.GetByIdAsync(999);
        Assert.IsNull(category);
    }
    // Tạo mới danh mục
    [Test]
    public async Task CreateAsync_ShouldAddNewCategory()
    {
        var newCategory = new Category { Name = "Sushi" };
        await _categoryService.CreateAsync(newCategory);
        var category = await _categoryService.GetByIdAsync(newCategory.Id);
        Assert.IsNotNull(category);
        Assert.AreEqual("Sushi", category.Name);
    }
    //Cập nhật danh mục
    // Nếu cập nhật thành công thì trả về danh mục đã được cập nhật
    // Nếu cập nhật thất bại thì trả về null
    [Test]
    public async Task UpdateAsync_ShouldModifyCategory()
    {
        var category = await _categoryService.GetByIdAsync(1);
        category.Name = "Updated Burger";
        await _categoryService.UpdateAsync(category);
        var updatedCategory = await _categoryService.GetByIdAsync(1);
        Assert.AreEqual("Updated Burger", updatedCategory.Name);
    }
    //Xóa danh mục
    [Test]
    public async Task DeleteAsync_ShouldRemoveCategory()
    {
        await _categoryService.DeleteAsync(1);
        var category = await _categoryService.GetByIdAsync(1);
        Assert.IsNull(category);
    }
    //Kiểm tra tồn tại danh mục
    [Test]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        var exists = await _categoryService.ExistsAsync(1);
        Assert.IsTrue(exists);
    }
    //Kiểm tra không tồn tại danh mục
    [Test]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        var exists = await _categoryService.ExistsAsync(999);
        Assert.IsFalse(exists);
    }
}
