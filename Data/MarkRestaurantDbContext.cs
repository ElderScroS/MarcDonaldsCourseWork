using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;

namespace MarkRestaurant.Data
{
    public class MarkRestaurantDbContext : IdentityDbContext<User>
    {
        public MarkRestaurantDbContext(DbContextOptions<MarkRestaurantDbContext> options) : base(options) { }

        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Product> Menu => Set<Product>(); 
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Card> Cards => Set<Card>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Card>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CartId, ci.ProductId });

            modelBuilder.Entity<Order>()
                .Property(b => b.UserId)
                .IsRequired();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");
        }

        public async Task SeedDataAsync()
        {
            if (!await Menu.AnyAsync())
            {
                var products = new List<Product>
                {
                    new("Burgers", "Double Big Mark(beef)", 13.75m, "/images/burgers/bigtasty.jpg", true),
                    new("Burgers", "Double Big Mark(chicken)", 13.75m, "/images/burgers/bigtasty.jpg", true),
                    new("Burgers", "Hamburger", 2.70m, "/images/burgers/hamburger.jpg", true),
                    new("Burgers", "Triple Mark", 6.00m, "/images/burgers/hamburger.jpg", true),
                    new("Burgers", "Cheeseburger", 2.95m, "/images/burgers/cheeseburger.jpg", true),
                    new("Burgers", "Triple Mark Cheeseburger", 7.25m, "/images/burgers/cheeseburger.jpg", true),
                    new("Potato", "French Frize(Extra)", 5.70m, "/images/potato/frize.jpg", true),
                    new("Potato", "French Frize(Big)", 5.20m, "/images/potato/frize.jpg", true),
                    new("Potato", "French Frize(Average)", 4.30m, "/images/potato/frize.jpg", true),
                    new("Potato", "French Frize(Small)", 3.00m, "/images/potato/frize.jpg", true),
                    new("Potato", "Mark potatoes(Big)", 5.70m, "/images/potato/rustic.jpg", true),
                    new("Potato", "Mark potatoes(Small)", 4.30m, "/images/potato/rustic.jpg", true),
                    new("Snacks", "Chicken Mark(5 p)", 6.50m, "/images/snacks/strips.jpg", true),
                    new("Snacks", "Chicken Mark(3 p)", 4.30m, "/images/snacks/strips.jpg", true),
                    new("Snacks", "Chicken Mark(8 p)", 10.50m, "/images/snacks/wings.jpg", true),
                    new("Snacks", "Chicken wings Mark(5 p)", 7.20m, "/images/snacks/wings.jpg", true),
                    new("Snacks", "Chicken wings Mark(3 p)", 4.95m, "/images/snacks/wings.jpg", true),
                    new("Snacks", "Chicken Box-Mark", 18.60m, "/images/snacks/box.jpg", true),
                    new("Drinks", "Coca-Mark 750ml", 4.30m, "/images/drinks/cola750.jpg", true),
                    new("Drinks", "Coca-Mark 400ml", 2.95m, "/images/drinks/cola750.jpg", true),
                    new("Drinks", "Coca-Mark 250ml", 2.40m, "/images/drinks/cola750.jpg", true),
                    new("Drinks", "Fanta 500ml", 3.75m, "/images/drinks/fanta750.jpg", true),
                    new("Drinks", "Fanta 400ml", 2.95m, "/images/drinks/fanta750.jpg", true),
                    new("Drinks", "Fanta 250ml", 2.40m, "/images/drinks/fanta750.jpg", true)
                };

                await Menu.AddRangeAsync(products);
                await SaveChangesAsync();
            }

            if (!await Admins.AnyAsync())
            {
                var passwordHasher = new PasswordHasher<Admin>();
                var admin = new Admin
                {
                    Username = "admin",
                    PasswordHash = passwordHasher.HashPassword(null!, "admin")
                };

                await Admins.AddAsync(admin);
                await SaveChangesAsync();
            }
            else
            {
                var existingAdmin = await Admins.FirstAsync();
                var passwordHasher = new PasswordHasher<Admin>();
                existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, "admin");
                await SaveChangesAsync();
            }
        }
    }
}
