using SeaBattle.Models;

namespace SeaBattle.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления пользователями
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Получает пользователя по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <returns>Объект пользователя или null, если пользователь не найден</returns>
        Task<User?> GetUserByUsername(string username);

        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Созданный объект пользователя</returns>
        Task<User> CreateUser(string username, string password);

        /// <summary>
        /// Проверяет учетные данные пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>true, если учетные данные верны, иначе false</returns>
        Task<bool> ValidateUser(string username, string password);
    }
} 