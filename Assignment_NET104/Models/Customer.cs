using System.ComponentModel.DataAnnotations;

namespace Assignment_NET104.Models
{
    public class Customer
    {
        public int Id { get; set; } // Khóa chính

        [Required]
        public string UserId { get; set; } // Liên kết tới bảng AspNetUsers của Identity

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; } // Lưu email khách hàng

        [Phone]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        // Navigation property: 1 khách có nhiều đơn hàng
        public ICollection<Order> Orders { get; set; }
    }
}
