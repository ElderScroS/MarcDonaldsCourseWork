using System.ComponentModel.DataAnnotations;

namespace MarkRestaurant
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(150)]
        public string Category { get; set; } = "";
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = "";
        [Required]
        public decimal Price { get; set; } = 0;
        [Required]
        public string ImageUrl { get; set; } = "/image/none.png";
        public bool InStock { get; set; } = false;

        public Product(string category, string title, decimal price, string imageUrl, bool inStock)
        {
            Id = Guid.NewGuid();
            Category = category;
            Title = title;
            Price = price;
            ImageUrl = imageUrl;
            InStock = inStock;
        }
    }
}
