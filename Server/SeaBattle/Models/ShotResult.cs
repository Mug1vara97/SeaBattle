namespace SeaBattle.Models
{
    /// <summary>
    /// Перечисление возможных результатов выстрела
    /// </summary>
    public enum ShotResult
    {
        /// <summary>
        /// Промах
        /// </summary>
        Error,

        /// <summary>
        /// Попадание
        /// </summary>
        Miss,

        /// <summary>
        /// Корабль уничтожен
        /// </summary>
        Hit,

        /// <summary>
        /// Корабль уничтожен
        /// </summary>
        Destroyed,

        /// <summary>
        /// Победа (последний корабль уничтожен)
        /// </summary>
        Win
    }
} 