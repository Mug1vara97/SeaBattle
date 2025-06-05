using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет рейтинг игрока в системе морского боя
    /// </summary>
    public class PlayerRanking
    {
        /// <summary>
        /// Уникальный идентификатор записи рейтинга
        /// </summary>
        [Key]
        public string PlayerUsername { get; set; } = string.Empty;

        public int Rating { get; set; }
        /// <summary>
        /// Количество побед игрока
        /// </summary>
        public int Wins { get; set; }
        /// <summary>
        /// Количество поражений игрока
        /// </summary>
        public int Losses { get; set; }
        /// <summary>
        /// Общее количество сыгранных игр
        /// </summary>
        public int TotalGames { get; set; }
    }
} 