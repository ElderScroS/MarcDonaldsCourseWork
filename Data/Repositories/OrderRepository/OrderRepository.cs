using MarkRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkRestaurant.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly ILogger<OrderRepository> _logger;
        private readonly IMemoryCache _cache;

        public OrderRepository(MarkRestaurantDbContext context, ILogger<OrderRepository> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            string cacheKey = $"order_{orderId}";

            if (!_cache.TryGetValue(cacheKey, out Order? order))
            {
                order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    _cache.Set(cacheKey, order, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
            }

            return order;
        }

        public async Task<List<Order>?> GetCompletedOrdersByUserIdAsync(string userId)
        {
            string cacheKey = $"completed_orders_{userId}";

            if (!_cache.TryGetValue(cacheKey, out List<Order>? orders))
            {
                orders = await _context.Orders
                    .Where(o => o.UserId == userId && o.IsCompleted)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Include(c => c.SendToAddress)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                if (orders.Any())
                {
                    _cache.Set(cacheKey, orders, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
            }

            return orders;
        }

        public async Task<Order> CreateOrderAsync(User user, bool leaveAtDoor)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.SendToAddress)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                _logger.LogError("Cart not found for user {UserId}", user.Id);
                throw new InvalidOperationException("Cart not found for user.");
            }

            var order = new Order
            {
                User = user,
                UserId = user.Id,
                SendToAddress = cart.SendToAddress,
                SendToAddressId = cart.SendToAddressId,
                Distance = cart.Distance,
                TipsPercentage = cart.TipsPercentage,
                TipsAmount = cart.TipsAmount,
                DeliveryCost = cart.DeliveryCost,
                DeliveryTime = cart.DeliveryTime,
                PaymentMethod = user.PaymentMethod,
                LeaveAtDoor = leaveAtDoor,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Order = order,
                    OrderId = order.Id,
                    Product = item.Product,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });
            }

            order.Amount = cart.Amount + cart.TipsAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _cache.Remove($"completed_orders_{user.Id}");

            return order;
        }

        public async Task FinishAllActiveOrdersAsync()
        {
            var activeOrders = await _context.Orders
                .Where(o => !o.IsCompleted)
                .ToListAsync();

            foreach (var order in activeOrders)
            {
                order.IsCompleted = true;
                order.DelieveredAt = DateTime.UtcNow;
                _cache.Remove($"order_{order.Id}");
                _cache.Remove($"completed_orders_{order.UserId}");
            }

            await _context.SaveChangesAsync();

            var userIds = activeOrders.Select(o => o.UserId).Distinct().ToList();

            var carts = await _context.Carts
                .Include(c => c.Items)
                .Where(c => userIds.Contains(c.UserId))
                .ToListAsync();

            foreach (var cart in carts)
            {
                _context.CartItems.RemoveRange(cart.Items);
            }

            await _context.SaveChangesAsync();
        }

        public async Task FinishOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsCompleted);

            if (order != null)
            {
                order.IsCompleted = true;
                order.DelieveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _cache.Remove($"order_{orderId}");
                _cache.Remove($"completed_orders_{order.UserId}");

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.Items);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}