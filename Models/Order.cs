using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkRestaurant.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; private set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DelieveredAt { get; set; } = DateTime.UtcNow;
        public int DeliveryTime { get; set; } 
        public bool IsCompleted { get; set; } = false;
        public decimal TipsPercentage { get; set; }
        public decimal TipsAmount { get; set; }
        public decimal Distance { get; set; }
        public decimal DeliveryCost { get; set; }
        public Guid? SendToAddressId { get; set; }
        [ForeignKey("SendToAddressId")]
        public Address? SendToAddress { get; set; }
        public bool LeaveAtDoor { get; set; } = false;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public Order()
        {
            OrderNumber = GenerateOrderNumberFromId(Id);
        }

        private string GenerateOrderNumberFromId(Guid id)
        {
            return "#" + id.ToString("N")[..7].ToUpper();
        }
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        ApplePay,
        GooglePay
    }
}
