using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkRestaurant.Models
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public decimal Distance { get; set; }
        public decimal DeliveryCost { get; set; }
        public int DeliveryTime { get; set; } 
        public decimal TipsPercentage { get; set; }
        public decimal TipsAmount { get; set; }       
        public bool LeaveAtDoor { get; set; } = false;
        public decimal Amount { get; set; }
        public Guid? SendToAddressId { get; set; }
        [ForeignKey("SendToAddressId")]
        public Address? SendToAddress { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        public Cart(string userId)
        {
            UserId = userId;
        }

        public Cart(string userId, ICollection<CartItem> items)
        {
            UserId = userId;
            Items = items;
        }
    }
}