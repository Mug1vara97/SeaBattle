using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет историю завершенной игры в морской бой
    /// </summary>
    public class GameHistory
    {
        /// <summary>
        /// Уникальный идентификатор записи в истории игр
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Имя игрока, создавшего игру
        /// </summary>
        [Required]
        public string PlayerUsername { get; set; }

        /// <summary>
        /// Идентификатор игровой сессии
        /// </summary>
        [Required]
        public string GameId { get; set; }

        /// <summary>
        /// Имя игрока, присоединившегося к игре
        /// </summary>
        public string? OpponentUsername { get; set; }

        /// <summary>
        /// Дата и время завершения игры
        /// </summary>
        public DateTime GameFinishedAt { get; set; }

        /// <summary>
        /// Имя победителя игры
        /// </summary>
        [Required]
        public string Result { get; set; }

        public GameHistory()
        {
            Id = Guid.NewGuid();
            GameFinishedAt = DateTime.UtcNow;
        }
    }
} 