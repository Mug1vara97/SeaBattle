using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SeaBattle.Hubs;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Controllers
{
    /// <summary>
    /// Контроллер для управления игровым процессом морского боя
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IHubContext<LobbyHub> _lobbyHub;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера игры
        /// </summary>
        /// <param name="gameService">Сервис для управления игровым процессом</param>
        /// <param name="lobbyHub">Хаб для работы с лобби</param>
        public GameController(IGameService gameService, IHubContext<LobbyHub> lobbyHub)
        {
            _gameService = gameService;
            _lobbyHub = lobbyHub;
        }

        /// <summary>
        /// Создает новую игру
        /// </summary>
        /// <param name="request">Данные для создания игры</param>
        /// <returns>Информация о созданной игре</returns>
        /// <response code="200">Игра успешно создана</response>
        [HttpPost("create")]
        public async Task<ActionResult<Game>> CreateGame([FromBody] CreateGameRequest request)
        {
            var game = await _gameService.CreateGame(request.CreatorName, request.IsOpenLobby);
            await _lobbyHub.Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
            return Ok(game);
        }

        /// <summary>
        /// Получает список открытых лобби
        /// </summary>
        /// <returns>Список доступных игровых лобби</returns>
        /// <response code="200">Список лобби успешно получен</response>
        [HttpGet("lobbies")]
        public async Task<ActionResult<IEnumerable<LobbyInfo>>> GetOpenLobbies()
        {
            var lobbies = await _gameService.GetOpenLobbies();
            return Ok(lobbies);
        }

        /// <summary>
        /// Присоединяет игрока к существующей игре
        /// </summary>
        /// <param name="request">Данные для присоединения к игре</param>
        /// <returns>Информация об игре</returns>
        /// <response code="200">Успешное присоединение к игре</response>
        /// <response code="404">Игра не найдена</response>
        [HttpPost("join")]
        public async Task<ActionResult<Game>> JoinGame([FromBody] JoinGameRequest request)
        {
            var game = await _gameService.JoinGame(request.GameId, request.OpponentName);
            if (game == null)
            {
                return NotFound("Игра не найдена");
            }
            await _lobbyHub.Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
            return Ok(game);
        }

        /// <summary>
        /// Устанавливает готовность игрока к началу игры
        /// </summary>
        /// <param name="request">Данные о готовности игрока</param>
        /// <returns>Обновленная информация об игре</returns>
        /// <response code="200">Статус готовности успешно обновлен</response>
        /// <response code="404">Игра не найдена</response>
        [HttpPost("ready")]
        public async Task<ActionResult<Game>> SetReady([FromBody] ReadyRequest request)
        {
            var game = await _gameService.SetReady(request.GameId, request.PlayerName);
            if (game == null)
            {
                return NotFound("Игра не найдена");
            }
            await _lobbyHub.Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
            return Ok(game);
        }

        /// <summary>
        /// Выполняет выстрел по указанной позиции
        /// </summary>
        /// <param name="request">Данные о выстреле</param>
        /// <returns>Результат выстрела</returns>
        /// <response code="200">Выстрел выполнен успешно</response>
        /// <response code="404">Игра не найдена</response>
        [HttpPost("shot")]
        public async Task<ActionResult<ShotResult>> MakeShot([FromBody] ShotRequest request)
        {
            var (game, result) = await _gameService.MakeShot(request.GameId, request.PlayerName, request.Position);
            if (game == null)
            {
                return NotFound("Игра не найдена");
            }
            return Ok(result);
        }

        /// <summary>
        /// Получает таблицу лидеров
        /// </summary>
        /// <param name="count">Количество записей для отображения</param>
        /// <returns>Список лучших игроков</returns>
        /// <response code="200">Таблица лидеров успешно получена</response>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<IEnumerable<PlayerRanking>>> GetLeaderboard([FromQuery] int count = 10)
        {
            var leaderboard = await _gameService.GetLeaderboardAsync(count);
            return Ok(leaderboard);
        }
    }

    /// <summary>
    /// Модель запроса для создания новой игры
    /// </summary>
    public class CreateGameRequest
    {
        /// <summary>
        /// Имя создателя игры
        /// </summary>
        public string CreatorName { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, указывающий является ли лобби открытым для присоединения
        /// </summary>
        public bool IsOpenLobby { get; set; }
    }

    /// <summary>
    /// Модель запроса для присоединения к игре
    /// </summary>
    public class JoinGameRequest
    {
        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public string GameId { get; set; } = string.Empty;

        /// <summary>
        /// Имя присоединяющегося игрока
        /// </summary>
        public string OpponentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель запроса для установки готовности игрока
    /// </summary>
    public class ReadyRequest
    {
        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public string GameId { get; set; } = string.Empty;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель запроса для выполнения выстрела
    /// </summary>
    public class ShotRequest
    {
        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public string GameId { get; set; } = string.Empty;

        /// <summary>
        /// Имя игрока, выполняющего выстрел
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Позиция выстрела на игровом поле
        /// </summary>
        public Position Position { get; set; } = new();
    }
} 