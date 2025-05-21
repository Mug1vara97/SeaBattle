using SeaBattle.Models;

namespace SeaBattle.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByUsername(string username);
        Task<User> CreateUser(string username, string password);
        Task<bool> ValidateUser(string username, string password);
    }
} 