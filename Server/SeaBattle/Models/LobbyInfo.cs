namespace SeaBattle.Models
{
    /// <summary>
    /// Представляет информацию о игровом лобби
    /// </summary>
    public class LobbyInfo
    {
        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public string GameId { get; set; } = string.Empty;

        /// <summary>
        /// Имя создателя лобби
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, указывающий является ли лобби открытым для присоединения
        /// </summary>
        public bool IsOpenLobby { get; set; }

        /// <summary>
        /// Дата и время создания лобби
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 