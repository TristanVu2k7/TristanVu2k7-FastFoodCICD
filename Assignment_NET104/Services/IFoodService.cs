using Microsoft.EntityFrameworkCore;
using Assignment_NET104.Models;

namespace Assignment_NET104.Services
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodItem>> GetAllAsync();
        Task<FoodItem> GetByIdAsync(int id);
        Task CreateAsync(FoodItem item);
        Task UpdateAsync(FoodItem item);
        Task DeleteAsync(int id);
        Task<IEnumerable<FoodItem>> SearchAsync(string keyword, int? categoryId, decimal? minPrice, decimal? maxPrice);
    }
}
