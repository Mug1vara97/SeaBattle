using SeaBattle.Models;

namespace SeaBattle.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления игровыми сессиями морского боя
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Создает новую игровую сессию
        /// </summary>
        /// <param name="creatorName">Имя создателя игры</param>
        /// <param name="isOpenLobby">Флаг открытого лобби</param>
        /// <returns>Созданная игровая сессия</returns>
        Task<Game> CreateGame(string creatorName, bool isOpenLobby);

        /// <summary>
        /// Присоединяет игрока к существующей игре
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="joinerName">Имя присоединяющегося игрока</param>
        /// <returns>Обновленная игровая сессия или null, если присоединение невозможно</returns>
        Task<Game?> JoinGame(string gameId, string joinerName);

        /// <summary>
        /// Устанавливает готовность игрока к началу игры
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        /// <returns>Обновленная игровая сессия или null при ошибке</returns>
        Task<Game?> SetReady(string gameId, string playerName);

        /// <summary>
        /// Получает информацию об игровой сессии
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <returns>Игровая сессия или null, если игра не найдена</returns>
        Task<Game?> GetGame(string gameId);

        /// <summary>
        /// Получает список открытых лобби
        /// </summary>
        /// <returns>Список доступных игровых сессий</returns>
        Task<List<Game>> GetOpenLobbies();

        /// <summary>
        /// Выполняет выстрел в указанную позицию
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя стреляющего игрока</param>
        /// <param name="position">Позиция выстрела</param>
        /// <returns>Кортеж с обновленной игрой и результатом выстрела</returns>
        Task<(Game? game, ShotResult result)> MakeShot(string gameId, string playerName, Position position);

        /// <summary>
        /// Получает историю игр пользователя
        /// </summary>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="count">Количество последних игр</param>
        /// <returns>Список записей истории игр</returns>
        Task<List<GameHistory>> GetPlayerGameHistory(string playerName, int count = 10);

        /// <summary>
        /// Получает таблицу лидеров
        /// </summary>
        /// <param name="topN">Количество лучших игроков</param>
        /// <returns>Список рейтингов игроков</returns>
        Task<List<PlayerRanking>> GetLeaderboardAsync(int topN);

        /// <summary>
        /// Размещает корабли на игровом поле
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="clientBoard">Расстановка кораблей</param>
        /// <returns>Обновленная игровая сессия или null при ошибке</returns>
        Task<Game?> PlaceShipsAsync(string gameId, string playerName, CellState[,] clientBoard);
    }
} 