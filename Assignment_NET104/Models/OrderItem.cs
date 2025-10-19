using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_NET104.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}
