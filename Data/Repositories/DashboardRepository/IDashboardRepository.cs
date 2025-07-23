using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public interface IDashboardRepository
    {
        // Кол-во зарегистрированных пользователей
        Task<int> GetAllUsersCountAsync();

        // Кол-во всех завершённых заказов
        Task<int> GetAllCompletedOrdersCountAsync();

        // Кол-во активных (не завершённых) заказов
        Task<int> GetActiveOrdersCountAsync();

        // Кол-во завершённых заказов за сегодня
        Task<int> GetTodayCompletedOrdersCountAsync();

        // Возвращает завершённые заказы за всё время
        Task<List<Order>> GetAllCompletedOrdersAsync();

        // Возвращает активные (не завершённые) заказы
        Task<List<Order>> GetActiveOrdersAsync();

        // Возвращает завершённые заказы за сегодня
        Task<List<Order>> GetTodayСompletedOrdersAsync();

        // Подсчет прибыли от 1 юзера
        Task<decimal> GetTotalSpentByUserAsync(User user);

        // Подсчет общей прибыли от всех заказов
        Task<decimal> GetTotalRevenueAsync();

        // Получить средний доход на одного пользователя
        Task<decimal> GetAverageRevenuePerUserAsync();

        // Топ 3 юзера
        Task<List<User>> GetTopUsersAsync();

        // Получить данные о продажах за неделю
        Task<List<DashboardRepository.SalesData>> GetWeeklySalesAsync();

        // Получить данные о регистрации пользователей за неделю
        Task<List<DashboardRepository.UserRegistrationData>> GetWeeklyUserRegistrationsAsync();
        
        void InvalidateUserRelatedCache();

    }
}
