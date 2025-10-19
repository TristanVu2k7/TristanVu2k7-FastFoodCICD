using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_NET104.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL hình ảnh")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "Link hình ảnh")]
        public string ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public int Quantity { get; set; } = 0;

    }
}
