namespace SeaBattle.Models
{
    /// <summary>
    /// Класс, представляющий информацию о лобби игры
    /// </summary>
    public class LobbyInfo
    {
        /// <summary>
        /// Идентификатор игровой сессии
        /// </summary>
       public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Имя создателя лобби
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Статус лобби
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время создания лобби
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 