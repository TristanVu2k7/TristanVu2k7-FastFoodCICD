using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Assignment_NET104.Models
{
    public class ApplicationUser : IdentityUser
    {


        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        public string Address { get; set; }
    }
}
