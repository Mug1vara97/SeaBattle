using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет рейтинг игрока в таблице лидеров
    /// </summary>
    public class PlayerRanking
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        [Key]
        public string PlayerUsername { get; set; } = string.Empty;

        public int Rating { get; set; }

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

        /// <summary>
        /// Процент побед
        /// </summary>
        public double WinRate => TotalGames == 0 ? 0 : (double)Wins / TotalGames * 100;
    }
} 