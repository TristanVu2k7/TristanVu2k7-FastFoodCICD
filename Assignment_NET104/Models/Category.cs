namespace Assignment_NET104.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Navigation property for related products
        public ICollection<FoodItem> FoodItems { get; set; } 
    }
}
