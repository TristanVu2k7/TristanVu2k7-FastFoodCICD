using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Assignment_NET104.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }
        public string CustomerName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
    }
}

