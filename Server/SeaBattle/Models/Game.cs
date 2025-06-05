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
        /// Уникальный идентификатор игровой сессии
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Имя игрока, создавшего игровую сессию
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Имя игрока, присоединившегося к игровой сессии
        /// </summary>
        public string? JoinerName { get; set; }

        /// <summary>
        /// Флаг, указывающий, является ли лобби открытым для присоединения
        /// </summary>
        public bool IsOpenLobby { get; set; }

        /// <summary>
        /// Текущее состояние игры
        /// </summary>
        public GameState State { get; set; } = GameState.WaitingForOpponent;

        /// <summary>
        /// Дата и время создания игровой сессии
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Имя игрока, чей ход в данный момент
        /// </summary>
        public string? CurrentTurn { get; set; }

        /// <summary>
        /// Имя победителя игры
        /// </summary>
        public string? Winner { get; set; }

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
        /// Флаг готовности создателя игры к началу
        /// </summary>
        public bool CreatorReady { get; set; }

        /// <summary>
        /// Флаг готовности присоединившегося игрока к началу
        /// </summary>
        public bool JoinerReady { get; set; }
        
        /// <summary>
        /// Флаг, указывающий, расставил ли создатель игры свои корабли
        /// </summary>
        public bool CreatorBoardSet { get; set; } = false;

        /// <summary>
        /// Флаг, указывающий, расставил ли присоединившийся игрок свои корабли
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

        /// <summary>
        /// Флаг, указывающий, завершена ли игра
        /// </summary>
        public bool IsGameEnded => State == GameState.Finished;
    }
} 