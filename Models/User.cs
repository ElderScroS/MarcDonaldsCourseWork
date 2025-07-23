using System.ComponentModel.DataAnnotations;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;

namespace MarkRestaurant
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Surname { get; set; }
        [MaxLength(100)]
        public string MiddleName { get; set; }
        [MaxLength(100)]
        public int Age { get; set; }
        public string ProfileImagePath { get; set; }
        public DateTime RegistrationDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        
        public User()
        {
            Email = "";
            UserName = Email;
            Name = "";
            Surname = "";
            MiddleName = "";
            Age = 0;
            PhoneNumber = "";
            ProfileImagePath = "/images/other/person.jpg";
            RegistrationDate = DateTime.UtcNow;
        }

        public User(string email, string passwordHash)
        {
            Email = email;
            PasswordHash = passwordHash;
            UserName = email;
            Name = "";
            Surname = "";
            MiddleName = "";
            Age = 0;
            PhoneNumber = "";
            ProfileImagePath = "/images/other/person.jpg";
            RegistrationDate = DateTime.UtcNow;
        }
    }
}
