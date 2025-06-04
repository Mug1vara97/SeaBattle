namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет позицию на игровом поле
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Координата строки (0-9)
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Координата столбца (0-9)
        /// </summary>
        public int Col { get; set; }
        public bool IsHit { get; set; }
    }
} 