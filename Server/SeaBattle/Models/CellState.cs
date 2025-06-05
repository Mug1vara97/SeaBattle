/// <summary>
/// Представляет состояние клетки на игровом поле морского боя
/// </summary>
namespace SeaBattle.Models
{
    public enum CellState
    {
        /// <summary>
        /// Пустая клетка
        /// </summary>
        Empty,

        /// <summary>
        /// Клетка содержит корабль
        /// </summary>
        Ship,

        /// <summary>
        /// Клетка, в которую был произведен выстрел (промах)
        /// </summary>
        Miss,

        /// <summary>
        /// Клетка с подбитым кораблем
        /// </summary>
        Hit
    }
} 