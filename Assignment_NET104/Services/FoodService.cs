using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Assignment_NET104.Services
{

    public class FoodService : IFoodService
    {

        private readonly AppDbContext _context;
        public FoodService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<FoodItem>> GetAllAsync()
        {
            return await _context.FoodItems.ToListAsync();
        }
        public async Task<FoodItem> GetByIdAsync(int id)
        {
            return await _context.FoodItems
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task CreateAsync(FoodItem food)
        {
            try
            {
                // Gán ảnh mặc định nếu chưa có
                if (string.IsNullOrWhiteSpace(food.ImagePath))
                {
                    food.ImagePath = "https://via.placeholder.com/150";
                }

                // Gán mô tả mặc định nếu null
                if (string.IsNullOrWhiteSpace(food.Description))
                {
                    food.Description = "Chưa có mô tả";
                }

                // Trạng thái mặc định là true
                food.IsAvailable = true;

                _context.FoodItems.Add(food);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu món ăn: " + ex.Message);
                throw; // Ném lỗi ra Controller để hiển thị thông báo
            }
        }


        public async Task UpdateAsync(FoodItem item)
        {
            _context.FoodItems.Update(item);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item != null)
            {
                _context.FoodItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FoodItem>> SearchAsync(string keyword, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.FoodItems.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(f => f.Name.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(f => f.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(f => f.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(f => f.Price <= maxPrice.Value);

            return await query.Include(f => f.Category).ToListAsync();
        }
    }
}
