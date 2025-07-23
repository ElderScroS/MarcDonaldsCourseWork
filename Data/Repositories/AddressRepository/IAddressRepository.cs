using System.Security.Claims;
using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetAddressesByUserAsync(ClaimsPrincipal _user);
        Task<Address?> GetSelectedAddressAsync(ClaimsPrincipal _user);
        Task<Address?> GetSelectedAddressAsync(User user);
        Task<Address?> GetAddressByIdAsync(Guid id);
        Task<bool> AddAddressAsync(Address address, User user);
        Task<bool> UpdateAddressAsync(Guid id, string entrance, string floorApartment, string comment);
        Task<bool> DeleteAddressAsync(Guid id, User user);
        Task<bool> SelectAddressAsync(Guid addressId, User user);
    }
}
