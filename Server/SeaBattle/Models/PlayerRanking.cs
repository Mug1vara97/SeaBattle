using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Models
{
    public class PlayerRanking
    {
        [Key]
        public string PlayerUsername { get; set; } = string.Empty;

        public int Rating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalGames { get; set; }
    }
} 