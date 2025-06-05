namespace SeaBattle.Models
{
    /// <summary>
    /// Перечисление возможных результатов выстрела
    /// </summary>
    public enum ShotResult
    {
        /// <summary>
        /// Ошибка при выполнении выстрела
        /// </summary>
        Error,

        /// <summary>
        /// Промах - выстрел в пустую клетку
        /// </summary>
        Miss,

        /// <summary>
        /// Попадание - выстрел в корабль
        /// </summary>
        Hit,

        /// <summary>
        /// Уничтожение - корабль полностью уничтожен
        /// </summary>
        Destroyed,

        /// <summary>
        /// Победа - последний корабль противника уничтожен
        /// </summary>
        Win
    }
} 