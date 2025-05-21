using System.Text.Json.Serialization;
using SeaBattle.Models.Converters;

namespace SeaBattle.Models
{
    public class Game
    {
        public string Id { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string? JoinerName { get; set; }
        public bool IsOpenLobby { get; set; }
        public GameState State { get; set; } = GameState.WaitingForOpponent;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CurrentTurn { get; set; }
        public string? Winner { get; set; }

        [JsonConverter(typeof(BoardConverter))]
        public CellState[,]? CreatorBoard { get; set; }

        [JsonConverter(typeof(BoardConverter))]
        public CellState[,]? JoinerBoard { get; set; }

        public bool CreatorReady { get; set; }
        public bool JoinerReady { get; set; }
        
        public bool CreatorBoardSet { get; set; } = false;
        public bool JoinerBoardSet { get; set; } = false;

        public List<Position> CreatorShots { get; set; } = new();
        public List<Position> JoinerShots { get; set; } = new();

        public bool IsGameEnded => State == GameState.Finished;
    }
} 