using System.Security.Claims;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MarkRestaurant.Data.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        public CardRepository(MarkRestaurantDbContext context, UserManager<User> userManager, IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        private string GetUserCardsCacheKey(string userId) => $"UserCards_{userId}";
        private string GetSelectedCardCacheKey(string userId) => $"SelectedCard_{userId}";

        public async Task<List<Card>> GetCardsByUserAsync(ClaimsPrincipal _user)
        {
            var user = await _userManager.GetUserAsync(_user);
            if (user == null) return new List<Card>();

            var cacheKey = GetUserCardsCacheKey(user.Id);

            if (!_cache.TryGetValue(cacheKey, out List<Card>? cards))
            {
                cards = await _context.Cards
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                _cache.Set(cacheKey, cards, TimeSpan.FromMinutes(10));
            }
            return cards!;
        }

        public async Task<Card?> GetSelectedCardAsync(ClaimsPrincipal _user)
        {
            var user = await _userManager.GetUserAsync(_user);
            if (user == null) return null;

            var cacheKey = GetSelectedCardCacheKey(user.Id);

            if (!_cache.TryGetValue(cacheKey, out Card? selectedCard))
            {
                selectedCard = await _context.Cards
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsSelected);

                if (selectedCard != null)
                {
                    _cache.Set(cacheKey, selectedCard, TimeSpan.FromMinutes(10));
                }
            }
            return selectedCard;
        }

        public async Task<Card?> GetCardByIdAsync(Guid id)
        {
            return await _context.Cards.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddCardAsync(Card card, User user)
        {
            card.UserId = user.Id;

            _context.Cards.Add(card);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                _cache.Remove(GetUserCardsCacheKey(user.Id));
                _cache.Remove(GetSelectedCardCacheKey(user.Id));
            }

            return result;
        }

        public async Task<bool> DeleteCardAsync(Guid id, User user)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (card == null) return false;

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            var remainingCards = await _context.Cards
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            if (remainingCards.Any())
            {
                foreach (var c in remainingCards)
                    c.IsSelected = false;

                var selectedCard = remainingCards.First();
                selectedCard.IsSelected = true;
            }
            else
            {
                user.PaymentMethod = PaymentMethod.Cash;
                await _userManager.UpdateAsync(user);
            }

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                _cache.Remove(GetUserCardsCacheKey(user.Id));
                _cache.Remove(GetSelectedCardCacheKey(user.Id));
            }

            return result;
        }

        public async Task<bool> SelectCardAsync(Guid cardId, User user)
        {
            var userCards = await _context.Cards
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            var targetCard = userCards.FirstOrDefault(c => c.Id == cardId);
            if (targetCard == null)
                return false;

            foreach (var card in userCards)
                card.IsSelected = (card.Id == cardId);

            user.PaymentMethod = PaymentMethod.Card;
            await _userManager.UpdateAsync(user);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                _cache.Remove(GetUserCardsCacheKey(user.Id));
                _cache.Remove(GetSelectedCardCacheKey(user.Id));
            }

            return result;
        }

        public async Task<bool> UnSelectAllCardsAsync(User user)
        {
            var userCards = await _context.Cards
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            foreach (var card in userCards)
                card.IsSelected = false;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                _cache.Remove(GetUserCardsCacheKey(user.Id));
                _cache.Remove(GetSelectedCardCacheKey(user.Id));
            }

            return result;
        }
    }
}
