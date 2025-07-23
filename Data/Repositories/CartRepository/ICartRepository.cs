using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public interface ICartRepository
    {
        Task<CartItem?> GetCartItemByProductId(string userId, Guid productId);
        Task CreateCart(string userId);
        Task<Cart> GetCartByUserId(string userId);
        Task SetAddressCart(string userId);
        Task<int> AddProductToCartOrIncreaseQuantity(string userId, Product product, int quantity = 1);
        Task<int> RemoveProductFromCartOrDecreaseQuantity(string userId, Guid cartItemId);
        Task ClearCart(string userId);
        Task SetTips(string userId, int tipsPercentage, decimal tipsAmount);
        Task<List<CartItem>> GetCartItems(string userId);
        Task<int> GetTotalCartItemsCount(string userId);
        Task<decimal> GetTotalPrice(string userId);
    }
}