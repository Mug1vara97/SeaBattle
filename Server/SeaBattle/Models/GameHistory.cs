using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет историю игры морского боя
    /// </summary>
    public class GameHistory
    {
        /// <summary>
        /// Уникальный идентификатор записи истории
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string PlayerUsername { get; set; }

        /// <summary>
        /// Идентификатор игры
        /// </summary>
        [Required]
        public string GameId { get; set; } = string.Empty;

        public string? OpponentUsername { get; set; }

        /// <summary>
        /// Дата и время завершения игры
        /// </summary>
        public DateTime GameFinishedAt { get; set; }

        [Required]
        public string Result { get; set; }

        /// <summary>
        /// Имя создателя игры
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Имя присоединившегося игрока
        /// </summary>
        public string JoinerName { get; set; } = string.Empty;

        /// <summary>
        /// Имя победителя игры
        /// </summary>
        public string Winner { get; set; } = string.Empty;

        /// <summary>
        /// Количество ходов в игре
        /// </summary>
        public int TotalMoves { get; set; }

        /// <summary>
        /// Дата и время начала игры
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Продолжительность игры в минутах
        /// </summary>
        public double DurationMinutes => (GameFinishedAt - StartedAt).TotalMinutes;

        public GameHistory()
        {
            Id = Guid.NewGuid();
            GameFinishedAt = DateTime.UtcNow;
        }
    }
} 