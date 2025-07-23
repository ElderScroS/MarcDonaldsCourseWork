using MarkRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace MarkRestaurant.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly ILogger<DashboardRepository> _logger;
        private readonly IMemoryCache _cache;

        public DashboardRepository(MarkRestaurantDbContext context, ILogger<DashboardRepository> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        private void InvalidateCache(params string[] keys)
        {
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        #region Count

        public async Task<int> GetAllUsersCountAsync()
        {
            return await _cache.GetOrCreateAsync("AllUsersCount", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.Users.CountAsync();
            });
        }

        public async Task<int> GetAllCompletedOrdersCountAsync()
        {
            return await _cache.GetOrCreateAsync("AllCompletedOrdersCount", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders.Where(o => o.IsCompleted).CountAsync();
            });
        }

        public async Task<int> GetActiveOrdersCountAsync()
        {
            return await _cache.GetOrCreateAsync("ActiveOrdersCount", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders.Where(o => !o.IsCompleted).CountAsync();
            });
        }

        public async Task<int> GetTodayCompletedOrdersCountAsync()
        {
            var cacheKey = $"TodayCompletedOrders_{DateTime.Today:yyyyMMdd}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders
                    .Where(o => o.CreatedAt.Date == DateTime.Today && o.IsCompleted)
                    .CountAsync();
            });
        }

        #endregion

        #region Get

        public async Task<List<Order>> GetAllCompletedOrdersAsync()
        {
            var orders = await _cache.GetOrCreateAsync("AllCompletedOrders", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.IsCompleted)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            });

            return orders!;
        }

        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            var orders = await _cache.GetOrCreateAsync("ActiveOrders", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => !o.IsCompleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            });

            return orders ?? new List<Order>();
        }


        public async Task<List<Order>> GetTodayСompletedOrdersAsync()
        {
            var cacheKey = $"TodayCompletedOrdersList_{DateTime.Today:yyyyMMdd}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Orders
                    .Where(o => o.IsCompleted && o.CreatedAt.Date == DateTime.UtcNow.Date)
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }) ?? new List<Order>();
        }

        #endregion

        #region Other

        public async Task<decimal> GetTotalSpentByUserAsync(User user)
        {
            var cacheKey = $"TotalSpentByUser_{user.Id}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .SumAsync(o => o.Amount);
            });
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _cache.GetOrCreateAsync("TotalRevenue", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var totalRevenue = await _context.OrderItems
                    .SumAsync(o => o.Product!.Price * o.Quantity);

                return Math.Round(totalRevenue);
            });
        }

        public async Task<decimal> GetAverageRevenuePerUserAsync()
        {
            return await _cache.GetOrCreateAsync("AverageRevenuePerUser", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var totalRevenue = await GetTotalRevenueAsync();
                var totalUsers = await GetAllUsersCountAsync();
                return totalUsers > 0 ? totalRevenue / totalUsers : 0;
            });
        }

        public async Task<List<User>> GetTopUsersAsync()
        {
            return await _cache.GetOrCreateAsync("TopUsers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var topUsers = await _context.Orders
                    .Where(o => o.IsCompleted)
                    .GroupBy(o => o.UserId)
                    .Select(group => new
                    {
                        UserId = group.Key,
                        OrderCount = group.Count()
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(3)
                    .ToListAsync();

                var userIds = topUsers.Select(x => x.UserId).ToList();

                return await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();
            }) ?? new List<User>();
        }

        public async Task<List<SalesData>> GetWeeklySalesAsync()
        {
            return await _cache.GetOrCreateAsync("WeeklySales", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var salesData = await _context.Orders
                    .Where(o => o.CreatedAt >= DateTime.Now.AddDays(-6))
                    .SelectMany(o => o.Items)
                    .GroupBy(oi => oi.Order!.CreatedAt.Date)
                    .Select(g => new SalesData
                    {
                        Day = g.Key.ToString("dddd", CultureInfo.InvariantCulture),
                        Date = g.Key.ToString("dd/MM/yyyy"),
                        TotalSales = Math.Round(g.Sum(oi => oi.Product!.Price * oi.Quantity), 2)
                    })
                    .ToListAsync();

                var result = new List<SalesData>();

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i).Date;
                    var salesForDay = salesData.FirstOrDefault(s => s.Date == date.ToString("dd/MM/yyyy"));

                    result.Add(salesForDay ?? new SalesData
                    {
                        Day = date.ToString("dddd", CultureInfo.InvariantCulture),
                        Date = date.ToString("dd/MM/yyyy"),
                        TotalSales = 0
                    });
                }

                return result;
            }) ?? new List<SalesData>();
        }

        public async Task<List<UserRegistrationData>> GetWeeklyUserRegistrationsAsync()
        {
            return await _cache.GetOrCreateAsync("WeeklyUserRegistrations", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var registrationData = await _context.Users
                    .Where(u => u.RegistrationDate >= DateTime.Now.AddDays(-6))
                    .GroupBy(u => u.RegistrationDate.Date)
                    .Select(g => new UserRegistrationData
                    {
                        Day = g.Key.ToString("dddd", CultureInfo.InvariantCulture),
                        Date = g.Key.ToString("dd/MM/yyyy"),
                        RegistrationCount = g.Count()
                    })
                    .ToListAsync();

                var result = new List<UserRegistrationData>();

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i).Date;
                    var reg = registrationData.FirstOrDefault(r => r.Date == date.ToString("dd/MM/yyyy"));

                    result.Add(reg ?? new UserRegistrationData
                    {
                        Day = date.ToString("dddd", CultureInfo.InvariantCulture),
                        Date = date.ToString("dd/MM/yyyy"),
                        RegistrationCount = 0
                    });
                }

                return result;
            }) ?? new List<UserRegistrationData>();
        }

        #endregion

        #region CacheInvalidationTriggers (пример использования)

        public void InvalidateOrderRelatedCache()
        {
            InvalidateCache(
                "AllCompletedOrders",
                "AllCompletedOrdersCount",
                "ActiveOrders",
                "ActiveOrdersCount",
                $"TodayCompletedOrders_{DateTime.Today:yyyyMMdd}",
                $"TodayCompletedOrdersList_{DateTime.Today:yyyyMMdd}",
                "WeeklySales",
                "TotalRevenue",
                "AverageRevenuePerUser",
                "TopUsers"
            );
        }

        public void InvalidateUserRelatedCache()
        {
            InvalidateCache(
                "AllUsersCount",
                "AverageRevenuePerUser",
                "TopUsers",
                "WeeklyUserRegistrations"
            );
        }

        #endregion

        public class SalesData
        {
            public string Day { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public decimal TotalSales { get; set; }
        }

        public class UserRegistrationData
        {
            public string Day { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public int RegistrationCount { get; set; }
        }
    }
}
