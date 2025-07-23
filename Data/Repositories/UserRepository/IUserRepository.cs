namespace MarkRestaurant.Data.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsers();
        Task<bool> DeleteUserFullyAsync(string email);
        Task<User> SaveChanges(User user, string name, string surname, string middleName, int age, string phoneNumber, IFormFile profileImage);
    }
}
