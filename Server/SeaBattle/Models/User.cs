using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет пользователя в системе
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        /// <summary>
        /// Хэш пароля пользователя
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Количество побед
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Количество поражений
        /// </summary>
        public int Losses { get; set; }

        /// <summary>
        /// Общее количество игр
        /// </summary>
        public int TotalGames => Wins + Losses;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 