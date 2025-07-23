using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkRestaurant.Models
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int Quantity { get; set; } = 1;
        
        public CartItem(Guid cartId, Guid productId, int quantity = 1)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}