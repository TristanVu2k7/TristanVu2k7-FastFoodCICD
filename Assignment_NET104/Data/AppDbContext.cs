using Assignment_NET104.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_NET104.Data
{
    // Kế thừa từ IdentityDbContext để có sẵn các bảng Identity
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Burger" },
                new Category { Id = 2, Name = "Pizza" }
            );
        }

        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
    }
}
