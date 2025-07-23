using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<List<Order>?> GetCompletedOrdersByUserIdAsync(string userId);
        Task<Order> CreateOrderAsync(User user, bool leaveAtDoor);
        Task FinishAllActiveOrdersAsync();
        Task FinishOrderAsync(Guid orderId);
    }
}