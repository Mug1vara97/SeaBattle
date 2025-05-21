using Microsoft.EntityFrameworkCore;
using SeaBattle.Data;
using SeaBattle.Models;
using System.Security.Cryptography;
using System.Text;

namespace SeaBattle.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }

        public async Task<User> CreateUser(string username, string password)
        {
            var passwordHash = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ValidateUser(string username, string password)
        {
            var user = await GetUserByUsername(username);
            if (user == null) return false;

            return HashPassword(password) == user.PasswordHash;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 