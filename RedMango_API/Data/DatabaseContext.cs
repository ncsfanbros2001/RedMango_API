using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Models;

namespace RedMango_API.Data
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        { 

        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<MenuItem>().HasData(new MenuItem
            {
                Id = 1,
                Name = "Spring Roll",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/spring roll.jpg",
                Price = 7.99,
                Category = "Appetizer",
                SpecialTag = ""
            }, new MenuItem
            {
                Id = 2,
                Name = "Idli",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/idli.jpg",
                Price = 8.99,
                Category = "Appetizer",
                SpecialTag = ""
            }, new MenuItem
            {
                Id = 3,
                Name = "Panu Puri",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/pani puri.jpg",
                Price = 8.99,
                Category = "Appetizer",
                SpecialTag = "Best Seller"
            }, new MenuItem
            {
                Id = 4,
                Name = "Hakka Noodles",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/hakka noodles.jpg",
                Price = 10.99,
                Category = "Entrée",
                SpecialTag = ""
            }, new MenuItem
            {
                Id = 5,
                Name = "Malai Kofta",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/malai kofta.jpg",
                Price = 12.99,
                Category = "Entrée",
                SpecialTag = "Top Rated"
            }, new MenuItem
            {
                Id = 6,
                Name = "Paneer Pizza",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/paneer pizza.jpg",
                Price = 11.99,
                Category = "Entrée",
                SpecialTag = ""
            }, new MenuItem
            {
                Id = 7,
                Name = "Paneer Tikka",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/paneer tikka.jpg",
                Price = 13.99,
                Category = "Entrée",
                SpecialTag = "Chef's Special"
            }, new MenuItem
            {
                Id = 8,
                Name = "Carrot Love",
                Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                Image = "https://foodphotos.blob.core.windows.net/foods/carrot love.jpg",
                Price = 4.99,
                Category = "Dessert",
                SpecialTag = ""
            });
        }
    }
}
