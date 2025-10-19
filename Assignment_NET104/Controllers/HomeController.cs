using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment_NET104.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFoodService _foodService;
        private readonly ICategoryService _categoryService;


        public HomeController(IFoodService foodService, ICategoryService categoryService)
        {
            _foodService = foodService;
            _categoryService = categoryService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet("search")]
        public async Task<IActionResult> Index(string search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var foods = await _foodService.SearchAsync(search, categoryId, minPrice, maxPrice);

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories ?? new List<Category>(); // đảm bảo không null

            return View(foods);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
