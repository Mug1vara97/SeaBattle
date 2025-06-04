using SeaBattle.Models;

namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет ответ на выполненный выстрел
    /// </summary>
    public class ShotResultResponse
    {
        /// <summary>
        /// Результат выстрела
        /// </summary>
        public ShotResult Result { get; set; }

        /// <summary>
        /// Позиция выстрела
        /// </summary>
        public Position Position { get; set; } = new();

        /// <summary>
        /// Флаг успешности попадания
        /// </summary>
        public bool IsHit { get; set; }

        /// <summary>
        /// Имя игрока, чей следующий ход
        /// </summary>
        public string? CurrentTurn { get; set; }

        /// <summary>
        /// Текущее состояние игры
        /// </summary>
        public GameState GameState { get; set; }

        /// <summary>
        /// Сообщение об ошибке, если она произошла
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Флаг успешности выполнения операции
        /// </summary>
        public bool Success { get; set; }
    }
} 