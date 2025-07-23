using Microsoft.EntityFrameworkCore;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace MarkRestaurant.Data.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly ILogger<CartRepository> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        public CartRepository(MarkRestaurantDbContext context, ILogger<CartRepository> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _cache = cache;
        }

        private string GetCartCacheKey(string userId) => $"Cart_{userId}";
        private string GetCartItemsCacheKey(string userId) => $"CartItems_{userId}";
        private string GetCartItemsCountCacheKey(string userId) => $"CartItemsCount_{userId}";
        private string GetCartTotalPriceCacheKey(string userId) => $"CartTotalPrice_{userId}";

        private void InvalidateCartCache(string userId)
        {
            _cache.Remove(GetCartCacheKey(userId));
            _cache.Remove(GetCartItemsCacheKey(userId));
            _cache.Remove(GetCartItemsCountCacheKey(userId));
            _cache.Remove(GetCartTotalPriceCacheKey(userId));
        }

        public async Task<CartItem?> GetCartItemByProductId(string userId, Guid productId)
        {
            var cart = await GetCartByUserId(userId);

            return cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        }

        public async Task CreateCart(string userId)
        {
            var cart = new Cart(userId, new List<CartItem>());
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            InvalidateCartCache(userId);
        }

        public async Task<Cart> GetCartByUserId(string userId)
        {
            var cart = await _cache.GetOrCreateAsync(GetCartCacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                    .Include(c => c.SendToAddress)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    await CreateCart(userId);

                    cart = await _context.Carts
                        .Include(c => c.Items)
                            .ThenInclude(i => i.Product)
                        .Include(c => c.SendToAddress)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart == null)
                    {
                        throw new InvalidOperationException($"Cart could not be created for user: {userId}");
                    }
                }

                cart.TipsAmount = 0;
                cart.TipsPercentage = 0;

                await GetTotalPrice(userId);

                await _context.SaveChangesAsync();

                return cart;
            });

            return cart!;
        }

        public async Task SetAddressCart(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var address = await _context.Addresses
                .Where(a => a.UserId == user!.Id && a.IsSelected)
                .FirstOrDefaultAsync();

            var cart = await _context.Carts
                .Include(c => c.SendToAddress)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (address == null || cart == null)
                return;

            cart.SendToAddress = address;
            cart.SendToAddressId = address.Id;

            await _context.SaveChangesAsync();

            double originLat = 40.41236787231929;
            double originLng = 49.84615389213683;

            double destLat = (double)cart.SendToAddress.Latitude!;
            double destLng = (double)cart.SendToAddress.Longitude!;

            decimal distanceKm = (decimal)GetDistanceInKm(originLat, originLng, destLat, destLng);

            cart.Distance = Math.Round(distanceKm, 1);

            if (distanceKm <= 2.0m)
            {
                cart.DeliveryCost = 0;
            }
            else
            {
                decimal rate = 0.7m;
                decimal extraDistance = distanceKm - 2.0m;
                cart.DeliveryCost = RoundToNearestQuarter(extraDistance * rate);
            }

            double deliverySpeedKmPerHour = 45.0;

            decimal deliveryTimeInMinutes = (decimal)(distanceKm / (decimal)deliverySpeedKmPerHour) * 60;

            decimal roundedDeliveryTimeInMinutes = RoundToNearestQuarter(deliveryTimeInMinutes);

            cart.DeliveryTime = (int)Math.Ceiling(roundedDeliveryTimeInMinutes);

            await GetTotalPrice(userId);

            await _context.SaveChangesAsync();

            InvalidateCartCache(userId);
        }

        public async Task<int> AddProductToCartOrIncreaseQuantity(string userId, Product product, int quantity = 1)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart(userId, new List<CartItem>());

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                await _context.SaveChangesAsync();

                InvalidateCartCache(userId);

                return existingItem.Quantity;
            }
            else
            {
                var newItem = new CartItem(cart.Id, product.Id)
                {
                    Quantity = quantity
                };

                cart.Items.Add(newItem);
                await _context.SaveChangesAsync();

                InvalidateCartCache(userId);

                return quantity;
            }
        }

        public async Task<int> RemoveProductFromCartOrDecreaseQuantity(string userId, Guid productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItem = await GetCartItemByProductId(userId, productId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.Id == cartItem!.Id);
                if (item != null)
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                        await _context.SaveChangesAsync();

                        InvalidateCartCache(userId);

                        return item.Quantity;
                    }
                    else
                    {
                        cart.Items.Remove(item);
                        _context.CartItems.Remove(item);
                        await _context.SaveChangesAsync();

                        InvalidateCartCache(userId);

                        return 0;
                    }
                }
            }

            return 0;
        }

        public async Task ClearCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();

                cart.Amount = 0;
                cart.LeaveAtDoor = false;
                cart.TipsAmount = 0;
                cart.TipsPercentage = 0;

                await _context.SaveChangesAsync();

                InvalidateCartCache(userId);
            }
        }

        public async Task SetTips(string userId, int tipsPercentage, decimal tipsAmount)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                cart.TipsPercentage = tipsPercentage;
                cart.TipsAmount = tipsAmount;

                await _context.SaveChangesAsync();

                InvalidateCartCache(userId);
            }
        }

        public async Task<List<CartItem>> GetCartItems(string userId)
        {
            var cartItems = await _cache.GetOrCreateAsync(GetCartItemsCacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                return cart?.Items.ToList() ?? new List<CartItem>();
            });

            return cartItems!;
        }

        public async Task<int> GetTotalCartItemsCount(string userId)
        {
            return await _cache.GetOrCreateAsync(GetCartItemsCountCacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                return cart?.Items.Sum(i => i.Quantity) ?? 0;
            });
        }

        public async Task<decimal> GetTotalPrice(string userId)
        {
            return await _cache.GetOrCreateAsync(GetCartTotalPriceCacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .Include(c => c.SendToAddress)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items == null || cart.Items.Count == 0)
                {
                    if (cart != null)
                    {
                        cart.Amount = 0;
                        await _context.SaveChangesAsync();
                    }
                    return 0;
                }

                // Считаем сумму товаров
                decimal itemsTotal = cart.Items
                    .Where(i => i.Product != null)
                    .Sum(i => i.Product!.Price * i.Quantity);

                // Добавляем сервисный сбор, упаковку и доставку
                decimal total = itemsTotal + 0.80m + 0.50m + cart.DeliveryCost;

                // Если товаров 4 и больше — применяем скидку 30% ко всей сумме
                if (cart.Items.Count >= 4)
                {
                    total *= 0.7m;
                }

                total = Math.Round(total, 2);

                cart.Amount = total;
                await _context.SaveChangesAsync();

                return cart.Amount;
            });
        }

        private double GetDistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        decimal RoundToNearestQuarter(decimal value)
        {
            return Math.Round(value * 4, MidpointRounding.AwayFromZero) / 4;
        }

        private double DegreesToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
