using System.Text.Json.Serialization;
using SeaBattle.Models.Converters;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет игровую сессию морского боя
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Уникальный идентификатор игры
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Имя создателя игры
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Имя присоединившегося игрока
        /// </summary>
        public string? JoinerName { get; set; }

        /// <summary>
        /// Игровое поле создателя игры
        /// </summary>
        [JsonConverter(typeof(BoardConverter))]
        public CellState[,]? CreatorBoard { get; set; }

        /// <summary>
        /// Игровое поле присоединившегося игрока
        /// </summary>
        [JsonConverter(typeof(BoardConverter))]
        public CellState[,]? JoinerBoard { get; set; }

        /// <summary>
        /// Текущее состояние игры
        /// </summary>
        public GameState State { get; set; } = GameState.WaitingForOpponent;

        /// <summary>
        /// Имя игрока, чей ход сейчас
        /// </summary>
        public string? CurrentTurn { get; set; }

        /// <summary>
        /// Имя победителя игры
        /// </summary>
        public string? Winner { get; set; }

        /// <summary>
        /// Флаг, указывающий является ли лобби открытым для присоединения
        /// </summary>
        public bool IsOpenLobby { get; set; }

        /// <summary>
        /// Флаг готовности создателя игры
        /// </summary>
        public bool CreatorReady { get; set; }

        /// <summary>
        /// Флаг готовности присоединившегося игрока
        /// </summary>
        public bool JoinerReady { get; set; }
        
        /// <summary>
        /// Флаг, указывающий установлены ли корабли создателем
        /// </summary>
        public bool CreatorBoardSet { get; set; } = false;

        /// <summary>
        /// Флаг, указывающий установлены ли корабли присоединившимся игроком
        /// </summary>
        public bool JoinerBoardSet { get; set; } = false;

        /// <summary>
        /// Список выстрелов создателя игры
        /// </summary>
        public List<Position> CreatorShots { get; set; } = new();

        /// <summary>
        /// Список выстрелов присоединившегося игрока
        /// </summary>
        public List<Position> JoinerShots { get; set; } = new();

        public bool IsGameEnded => State == GameState.Finished;
    }
} 