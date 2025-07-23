using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MarkRestaurant.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MarkRestaurant.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        public UserRepository(MarkRestaurantDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager, IMemoryCache cache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _cache = cache; // инициализируем
        }

        public async Task<List<User>> GetAllUsers()
        {
            var cacheKey = "all_users";

            if (!_cache.TryGetValue(cacheKey, out List<User>? users))
            {
                users = await _context.Users.ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, users, cacheEntryOptions);
            }

            return users!;
        }

        public async Task<bool> DeleteUserFullyAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return false;

            // Удаляем Cart и связанные CartItems
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);
            }

            // Удаляем Orders и связанные OrderItems
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            foreach (var order in orders)
            {
                _context.OrderItems.RemoveRange(order.Items);
            }

            _context.Orders.RemoveRange(orders);

            // Удаляем адреса
            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            _context.Addresses.RemoveRange(addresses);

            // Удаляем карты
            var cards = await _context.Cards
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            _context.Cards.RemoveRange(cards);

            // Удаляем аватар из файловой системы
            if (!string.IsNullOrEmpty(user.ProfileImagePath) && user.ProfileImagePath != "/images/other/person.jpg")
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            _cache.Remove("all_users");

            return result.Succeeded;
        }
        
        public async Task<User> SaveChanges(User user, string name, string surname, string middleName, int age, string phoneNumber, IFormFile profileImage)
        {
            user!.Name = name;
            user.Surname = surname;
            user.MiddleName = middleName;
            user.Age = age;
            user.PhoneNumber = phoneNumber;

            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "usersImg");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(user.ProfileImagePath) && user.ProfileImagePath != "/images/other/person.jpg")
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                string fileExtension = Path.GetExtension(profileImage.FileName);
                string uniqueFileName = $"user_{user.Id}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                user.ProfileImagePath = $"/usersImg/{uniqueFileName}";
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);        

            if (cart == null) 
            {
                cart = new Cart(user.Id, new List<CartItem>());

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync(); 
            }

            _cache.Remove("all_users");

            return user;
        }
    }
}
