using System.Security.Claims;
using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public interface ICardRepository
    {
        Task<List<Card>> GetCardsByUserAsync(ClaimsPrincipal _user);
        Task<Card?> GetSelectedCardAsync(ClaimsPrincipal _user);
        Task<Card?> GetCardByIdAsync(Guid id);
        Task<bool> AddCardAsync(Card card, User user);
        Task<bool> DeleteCardAsync(Guid id, User user);
        Task<bool> SelectCardAsync(Guid cardId, User user);
        Task<bool> UnSelectAllCardsAsync(User user);
    }
}
