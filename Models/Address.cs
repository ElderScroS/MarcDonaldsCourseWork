using System.ComponentModel.DataAnnotations;

namespace MarkRestaurant
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; }
        public string? Title { get; set; } = "";
        public string? City { get; set; } = "";
        public string? Street { get; set; } = "";
        public string? HouseNumber { get; set; } = "";
        public string? FloorApartment { get; set; } = "";
        public string? Entrance { get; set; } = "";
        public string? Comment { get; set; } = "";
        public double? Latitude { get; set; } = 0;
        public double? Longitude { get; set; } = 0;

        public bool IsSelected { get; set; } = false;

        public string? UserId { get; set; }
        public User? User { get; set; }

        public Address()
        {
            Id = Guid.NewGuid();
        }
    }
}
