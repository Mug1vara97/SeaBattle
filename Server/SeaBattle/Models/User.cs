using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет пользователя в системе морского боя
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя, используемое для входа и отображения в игре
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        /// <summary>
        /// Хэшированный пароль пользователя для аутентификации
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 