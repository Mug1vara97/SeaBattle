using SeaBattle.Models;

namespace SeaBattle.Models
{
    public class ShotResultResponse
    {
        public Game? Game { get; set; }
        public ShotResult Result { get; set; }
        public Position? Position { get; set; }
        public bool IsHit { get; set; }
        public string? CurrentTurn { get; set; }
        public GameState GameState { get; set; }
    }
} 