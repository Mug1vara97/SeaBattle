namespace SeaBattle.Models
{
    /// <summary>
    /// Перечисление статусов игры для отображения в интерфейсе
    /// </summary>
    public enum GameStatus
    {
        /// <summary>
        /// ООжидание готовности игрока
        /// </summary>
        WaitingForOpponent,

        /// <summary>
        /// Ожидание готовности игрока
        /// </summary>
        WaitingForReady,

        /// <summary>
        /// Игра в процессе
        /// </summary>
        InProgress,

        /// <summary>
        /// Игра завершена
        /// </summary>
        Finished
    }
} 