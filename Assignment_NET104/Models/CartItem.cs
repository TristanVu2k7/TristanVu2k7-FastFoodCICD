namespace Assignment_NET104.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}
