using MarkRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MarkRestaurant.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly ILogger<AddressRepository> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IMemoryCache _cache;

        public AddressRepository(
            MarkRestaurantDbContext context, 
            ILogger<AddressRepository> logger, 
            UserManager<User> userManager, 
            ICartRepository cartRepository,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _cache = cache;
        }

        private string GetUserAddressesCacheKey(string userId) => $"UserAddresses_{userId}";
        private string GetSelectedAddressCacheKey(string userId) => $"SelectedAddress_{userId}";

        public async Task<List<Address>> GetAddressesByUserAsync(ClaimsPrincipal _user)
        {
            var user = await _userManager.GetUserAsync(_user);
            if (user == null) return new List<Address>();

            var cacheKey = GetUserAddressesCacheKey(user.Id);

            if (!_cache.TryGetValue(cacheKey, out List<Address>? addresses))
            {
                addresses = await _context.Addresses
                    .Where(a => a.UserId == user.Id)
                    .ToListAsync();

                _cache.Set(cacheKey, addresses, TimeSpan.FromMinutes(10));
            }

            return addresses!;
        }

        public async Task<Address?> GetSelectedAddressAsync(ClaimsPrincipal _user)
        {
            var user = await _userManager.GetUserAsync(_user);
            if (user == null) return null;

            var cacheKey = GetSelectedAddressCacheKey(user.Id);

            if (!_cache.TryGetValue(cacheKey, out Address? selectedAddress))
            {
                selectedAddress = await _context.Addresses
                    .Where(a => a.UserId == user.Id && a.IsSelected)
                    .FirstOrDefaultAsync();

                if (selectedAddress != null)
                    _cache.Set(cacheKey, selectedAddress, TimeSpan.FromMinutes(10));
            }

            return selectedAddress;
        }

        public async Task<Address?> GetSelectedAddressAsync(User user)
        {
            var cacheKey = GetSelectedAddressCacheKey(user.Id);

            if (!_cache.TryGetValue(cacheKey, out Address? selectedAddress))
            {
                selectedAddress = await _context.Addresses
                    .Where(a => a.UserId == user.Id && a.IsSelected)
                    .FirstOrDefaultAsync();

                if (selectedAddress != null)
                    _cache.Set(cacheKey, selectedAddress, TimeSpan.FromMinutes(10));
            }

            return selectedAddress;
        }

        public async Task<Address?> GetAddressByIdAsync(Guid id)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> AddAddressAsync(Address address, User user)
        {
            address.UserId = user.Id;

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            _cache.Remove(GetUserAddressesCacheKey(user.Id));
            _cache.Remove(GetSelectedAddressCacheKey(user.Id));

            return true;
        }

        public async Task<bool> UpdateAddressAsync(Guid id, string entrance, string floorApartment, string comment)
        {
            var existing = await _context.Addresses.FindAsync(id);

            if (existing == null) return false;

            existing.Entrance = entrance;
            existing.FloorApartment = floorApartment;
            existing.Comment = comment;

            await _context.SaveChangesAsync();

            _cache.Remove(GetUserAddressesCacheKey(existing!.UserId!));
            _cache.Remove(GetSelectedAddressCacheKey(existing.UserId!));

            return true;
        }

        public async Task<bool> DeleteAddressAsync(Guid id, User user)
        {
            var addressToDelete = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (addressToDelete == null)
                return false;

            var userAddresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            bool isLastAddress = userAddresses.Count == 1;

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart != null)
            {
                cart.SendToAddressId = null;
                cart.SendToAddress = null;
            }

            if (isLastAddress)
            {
                await _cartRepository.ClearCart(user.Id);
            }
            else if (addressToDelete.IsSelected)
            {
                var fallbackAddress = userAddresses
                    .Where(a => a.Id != id)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault();

                if (fallbackAddress != null)
                    fallbackAddress.IsSelected = true;

                addressToDelete.IsSelected = false;
                await _context.SaveChangesAsync();
            }

            _context.Addresses.Remove(addressToDelete);
            await _context.SaveChangesAsync();

            _cache.Remove(GetUserAddressesCacheKey(user.Id));
            _cache.Remove(GetSelectedAddressCacheKey(user.Id));

            return !isLastAddress;
        }

        public async Task<bool> SelectAddressAsync(Guid addressId, User user)
        {
            var userId = user.Id;

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            foreach (var addr in addresses)
                addr.IsSelected = (addr.Id == addressId);

            await _context.SaveChangesAsync();

            _cache.Remove(GetUserAddressesCacheKey(userId));
            _cache.Remove(GetSelectedAddressCacheKey(userId));

            return true;
        }
    }
}
