using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkRestaurant.Models
{
    public class Card
    {
        [Key]
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public string? CardName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisa { get; set; }

        public Card()
        {
            Id = Guid.NewGuid();
        }
    }
}