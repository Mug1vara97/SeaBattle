using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Models
{
    public class GameHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string PlayerUsername { get; set; }

        [Required]
        public string GameId { get; set; } 

        public string? OpponentUsername { get; set; } 

        public DateTime GameFinishedAt { get; set; }

        [Required]
        public string Result { get; set; }

        public GameHistory()
        {
            Id = Guid.NewGuid();
            GameFinishedAt = DateTime.UtcNow;
        }
    }
} 