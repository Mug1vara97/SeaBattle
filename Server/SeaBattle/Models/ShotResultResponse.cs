using SeaBattle.Models;

namespace SeaBattle.Models
{
    /// <summary>
    /// Класс, представляющий ответ на выстрел игрока
    /// </summary>
    public class ShotResultResponse
    {
        /// <summary>
        /// Текущее состояние игры после выстрела
        /// </summary>
        public Game? Game { get; set; }

        /// <summary>
        /// Результат выстрела
        /// </summary>
        public ShotResult Result { get; set; }

        /// <summary>
        /// Позиция выстрела на игровом поле
        /// </summary>
        public Position? Position { get; set; }

        /// <summary>
        /// Флаг, указывающий, было ли попадание
        /// </summary>
        public bool IsHit { get; set; }

        /// <summary>
        /// Имя игрока, чей ход следующий
        /// </summary>
        public string? CurrentTurn { get; set; }

        /// <summary>
        /// Текущее состояние игры
        /// </summary>
        public GameState GameState { get; set; }
    }
} 