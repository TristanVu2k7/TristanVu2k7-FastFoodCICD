using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_NET104.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Identity User ID
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> OrderItems { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Delivering = 2,
        Completed = 3,
        Cancelled = 4
    }
}
