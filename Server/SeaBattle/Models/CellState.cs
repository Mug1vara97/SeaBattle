namespace SeaBattle.Models
{
    /// <summary>
    /// Перечисление возможных состояний клетки игрового поля
    /// </summary>
    public enum CellState
    {
        /// <summary>
        /// Пустая клетка
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Клетка с кораблем
        /// </summary>
        Ship = 1,

        /// <summary>
        /// Клетка с попаданием
        /// </summary>
        Hit = 2,

        /// <summary>
        /// Клетка с промахом
        /// </summary>
        Miss = 3
    }
} 