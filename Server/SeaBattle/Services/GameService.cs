using SeaBattle.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SeaBattle.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SeaBattle.Services
{
    /// <summary>
    /// Реализация сервиса для управления игровыми сессиями морского боя.
    /// Предоставляет функционал для создания, управления и завершения игровых сессий,
    /// а также обработки игровой механики и ведения статистики.
    /// </summary>
    /// <remarks>
    /// Сервис обеспечивает:
    /// <list type="bullet">
    /// <item><description>Управление жизненным циклом игры (создание, присоединение, начало, завершение)</description></item>
    /// <item><description>Обработку игровой механики (размещение кораблей, выстрелы, определение попаданий)</description></item>
    /// <item><description>Ведение статистики и рейтинга игроков</description></item>
    /// <item><description>Хранение истории игр</description></item>
    /// </list>
    /// 
    /// Пример использования:
    /// <code>
    /// var gameService = new GameService(logger, dbContext);
    /// 
    /// // Создание новой игры
    /// var game = await gameService.CreateGame("Player1", true);
    /// 
    /// // Присоединение второго игрока
    /// await gameService.JoinGame(game.Id, "Player2");
    /// 
    /// // Размещение кораблей
    /// await gameService.PlaceShipsAsync(game.Id, "Player1", board1);
    /// await gameService.PlaceShipsAsync(game.Id, "Player2", board2);
    /// 
    /// // Выполнение хода
    /// var (updatedGame, result) = await gameService.MakeShot(game.Id, "Player1", new Position(0, 0));
    /// </code>
    /// </remarks>
    public class GameService : IGameService
    {
        private static readonly ConcurrentDictionary<string, Game> _games = new();
        private readonly ILogger<GameService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private const int InitialRating = 1000;
        private const int RatingChangeOnWin = 15;
        private const int RatingChangeOnLoss = 10;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса игры.
        /// </summary>
        /// <param name="logger">Сервис логирования для записи событий игры</param>
        /// <param name="dbContext">Контекст базы данных для сохранения состояния игры и статистики</param>
        /// <remarks>
        /// При инициализации сервис настраивает:
        /// <list type="bullet">
        /// <item><description>Систему логирования для отслеживания игровых событий</description></item>
        /// <item><description>Подключение к базе данных для сохранения статистики</description></item>
        /// </list>
        /// </remarks>
        public GameService(ILogger<GameService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Создает новую игровую сессию с указанным создателем.
        /// </summary>
        /// <param name="creatorName">Имя создателя игры</param>
        /// <param name="isOpenLobby">Флаг, указывающий, является ли лобби открытым для присоединения</param>
        /// <returns>Созданная игровая сессия</returns>
        /// <remarks>
        /// Метод выполняет следующие действия:
        /// <list type="number">
        /// <item><description>Генерирует уникальный идентификатор игры</description></item>
        /// <item><description>Инициализирует начальное состояние игры</description></item>
        /// <item><description>Добавляет игру в словарь активных игр</description></item>
        /// </list>
        /// 
        /// Начальное состояние игры включает:
        /// <list type="bullet">
        /// <item><description>Пустые игровые поля для обоих игроков</description></item>
        /// <item><description>Статус ожидания второго игрока</description></item>
        /// <item><description>Пустые списки выстрелов</description></item>
        /// </list>
        /// </remarks>
        public Task<Game> CreateGame(string creatorName, bool isOpenLobby)
        {
            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                CreatorName = creatorName,
                IsOpenLobby = isOpenLobby,
                State = GameState.WaitingForOpponent,
                CreatedAt = DateTime.UtcNow,
                CreatorBoard = null,
                JoinerBoard = null,
                CreatorReady = false,
                JoinerReady = false,
                CreatorBoardSet = false,
                JoinerBoardSet = false,
                CreatorShots = new List<Position>(),
                JoinerShots = new List<Position>(),
                CurrentTurn = creatorName
            };

            if (_games.TryAdd(game.Id, game))
            {
                _logger.LogInformation($"Game created: {game.Id} by {creatorName}. Waiting for opponent and setup.");
                return Task.FromResult(game);
            }
            _logger.LogError($"Failed to add game {game.Id} to dictionary.");
            _games.TryGetValue(game.Id, out var existingGame);
            return Task.FromResult(existingGame ?? game);
        }

        /// <summary>
        /// Размещает корабли на игровом поле указанного игрока.
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="clientBoard">Расстановка кораблей на поле</param>
        /// <returns>Обновленная игровая сессия или null при ошибке</returns>
        /// <remarks>
        /// Процесс размещения кораблей:
        /// <list type="number">
        /// <item><description>Проверка существования игры и прав игрока</description></item>
        /// <item><description>Валидация расстановки кораблей</description></item>
        /// <item><description>Сохранение расстановки в игровой сессии</description></item>
        /// </list>
        /// 
        /// Возможные ошибки:
        /// <list type="bullet">
        /// <item><description>Игра не найдена</description></item>
        /// <item><description>Игрок не является участником игры</description></item>
        /// <item><description>Корабли уже размещены</description></item>
        /// </list>
        /// </remarks>
        public Task<Game?> PlaceShipsAsync(string gameId, string playerName, CellState[,] clientBoard)
        {
            if (!_games.TryGetValue(gameId, out var game))
            {
                _logger.LogWarning($"PlaceShipsAsync: Game {gameId} not found.");
                return Task.FromResult<Game?>(null);
            }

            if (game.CreatorName == playerName)
            {
                if (game.CreatorBoardSet)
                {
                    _logger.LogWarning($"PlaceShipsAsync: Creator {playerName} already placed ships in game {gameId}.");
                    return Task.FromResult<Game?>(game);
                }
                game.CreatorBoard = clientBoard;
                game.CreatorBoardSet = true;
                _logger.LogInformation($"Creator {playerName} placed ships for game {gameId}.");
            }
            else if (game.JoinerName == playerName)
            {
                if (game.JoinerBoardSet)
                {
                    _logger.LogWarning($"PlaceShipsAsync: Joiner {playerName} already placed ships in game {gameId}.");
                    return Task.FromResult<Game?>(game);
                }
                game.JoinerBoard = clientBoard;
                game.JoinerBoardSet = true;
                _logger.LogInformation($"Joiner {playerName} placed ships for game {gameId}.");
            }
            else
            {
                _logger.LogWarning($"PlaceShipsAsync: Player {playerName} not part of game {gameId}.");
                return Task.FromResult<Game?>(null);
            }

            if (game.CreatorBoardSet && game.JoinerBoardSet)
            {
                _logger.LogInformation($"Both players have placed ships in game {gameId}. Ready for players to confirm start.");
            }
            
            return Task.FromResult<Game?>(game);
        }

        /// <inheritdoc/>
        public Task<Game?> JoinGame(string gameId, string joinerName)
        {
            if (!_games.TryGetValue(gameId, out var game))
            {
                return Task.FromResult<Game?>(null);
            }

            if (game.State != GameState.WaitingForOpponent || game.JoinerName != null)
            {
                return Task.FromResult<Game?>(null);
            }

            game.JoinerName = joinerName;
            game.State = GameState.WaitingForReady;
            return Task.FromResult<Game?>(game);
        }

        /// <inheritdoc/>
        public Task<Game?> SetReady(string gameId, string playerName)
        {
            if (!_games.TryGetValue(gameId, out var game))
            {
                return Task.FromResult<Game?>(null);
            }

            if (game.CreatorName == playerName)
            {
                game.CreatorReady = true;
            }
            else if (game.JoinerName == playerName)
            {
                game.JoinerReady = true;
            }

            if (game.CreatorReady && game.JoinerReady)
            {
                game.State = GameState.InProgress;
                if (string.IsNullOrEmpty(game.CurrentTurn))
                {
                    game.CurrentTurn = game.CreatorName;
                }
                _logger.LogInformation($"Game {gameId} started. First turn: {game.CurrentTurn}");
            }

            return Task.FromResult<Game?>(game);
        }

        /// <inheritdoc/>
        public Task<Game?> GetGame(string gameId)
        {
            return Task.FromResult(_games.TryGetValue(gameId, out var game) ? game : null);
        }

        /// <inheritdoc/>
        public Task<List<Game>> GetOpenLobbies()
        {
            return Task.FromResult(_games.Values
                .Where(g => g.IsOpenLobby && g.State == GameState.WaitingForOpponent)
                .ToList());
        }

        /// <summary>
        /// Обновляет рейтинг игрока после игры
        /// </summary>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="isWinner">Флаг победы</param>
        private async Task UpdatePlayerRankingAsync(string playerName, bool isWinner)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                _logger.LogWarning("UpdatePlayerRankingAsync: Player name is null or empty.");
                return;
            }

            var ranking = await _dbContext.PlayerRankings.FirstOrDefaultAsync(r => r.PlayerUsername == playerName);

            if (ranking == null)
            {
                ranking = new PlayerRanking
                {
                    PlayerUsername = playerName,
                    Rating = InitialRating,
                    Wins = 0,
                    Losses = 0,
                    TotalGames = 0
                };
                _dbContext.PlayerRankings.Add(ranking);
            }

            ranking.TotalGames++;
            if (isWinner)
            {
                ranking.Wins++;
                ranking.Rating += RatingChangeOnWin;
            }
            else
            {
                ranking.Losses++;
                ranking.Rating -= RatingChangeOnLoss;
                if (ranking.Rating < 0)
                {
                    ranking.Rating = 0;
                }
            }
        }

        /// <summary>
        /// Добавляет запись об игре в историю
        /// </summary>
        /// <param name="game">Игровая сессия</param>
        /// <param name="winnerUsername">Имя победителя</param>
        /// <param name="loserUsername">Имя проигравшего</param>
        public async Task AddGameToHistory(Game game, string winnerUsername, string loserUsername)
        {
            if (game == null || string.IsNullOrEmpty(winnerUsername) || string.IsNullOrEmpty(loserUsername))
            {
                _logger.LogWarning("AddGameToHistory: Invalid data provided.");
                return;
            }

            var winnerHistory = new GameHistory
            {
                PlayerUsername = winnerUsername,
                GameId = game.Id,
                OpponentUsername = loserUsername,
                Result = "Победа",
                GameFinishedAt = DateTime.UtcNow
            };

            var loserHistory = new GameHistory
            {
                PlayerUsername = loserUsername,
                GameId = game.Id,
                OpponentUsername = winnerUsername,
                Result = "Поражение",
                GameFinishedAt = DateTime.UtcNow
            };

            _dbContext.GameHistories.Add(winnerHistory);
            _dbContext.GameHistories.Add(loserHistory);

            await UpdatePlayerRankingAsync(winnerUsername, true);
            await UpdatePlayerRankingAsync(loserUsername, false);
            
            await _dbContext.SaveChangesAsync(); 
            _logger.LogInformation($"Game {game.Id} added to history and rankings updated for players {winnerUsername} and {loserUsername}.");
        }

        /// <summary>
        /// Выполняет выстрел в указанную позицию на поле противника.
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя стреляющего игрока</param>
        /// <param name="position">Координаты выстрела</param>
        /// <returns>Кортеж, содержащий обновленную игру и результат выстрела</returns>
        /// <remarks>
        /// Процесс обработки выстрела:
        /// <list type="number">
        /// <item><description>Проверка валидности хода (очередь игрока, корректность координат)</description></item>
        /// <item><description>Определение результата выстрела (промах/попадание/уничтожение)</description></item>
        /// <item><description>Обновление состояния игры</description></item>
        /// <item><description>Проверка условий победы</description></item>
        /// </list>
        /// 
        /// Возможные результаты:
        /// <list type="bullet">
        /// <item><description>Miss - промах</description></item>
        /// <item><description>Hit - попадание</description></item>
        /// <item><description>Destroyed - корабль уничтожен</description></item>
        /// <item><description>Win - победа (все корабли уничтожены)</description></item>
        /// <item><description>Error - ошибка</description></item>
        /// </list>
        /// 
        /// При победе:
        /// <list type="bullet">
        /// <item><description>Обновляется рейтинг игроков</description></item>
        /// <item><description>Сохраняется запись в истории игр</description></item>
        /// <item><description>Игра переходит в состояние Finished</description></item>
        /// </list>
        /// </remarks>
        public async Task<(Game? game, ShotResult result)> MakeShot(string gameId, string playerName, Position position)
        {
            if (!_games.TryGetValue(gameId, out var game))
            {
                _logger.LogError($"Game {gameId} not found");
                return (null, ShotResult.Error);
            }

            if (game.State != GameState.InProgress || game.CurrentTurn != playerName)
            {
                _logger.LogError($"Invalid game state or turn. State: {game.State}, CurrentTurn: {game.CurrentTurn}, Player: {playerName}");
                return (game, ShotResult.Error);
            }

            var targetBoard = playerName == game.CreatorName ? game.JoinerBoard : game.CreatorBoard;
            var shots = playerName == game.CreatorName ? game.CreatorShots : game.JoinerShots;

            if (position.Row < 0 || position.Row >= 10 || position.Col < 0 || position.Col >= 10)
            {
                _logger.LogError($"Invalid position: ({position.Row}, {position.Col})");
                return (game, ShotResult.Error);
            }

            if (shots.Any(s => s.Row == position.Row && s.Col == position.Col))
            {
                _logger.LogError($"Position already shot: ({position.Row}, {position.Col})");
                return (game, ShotResult.Error);
            }

            var shot = new Position { Row = position.Row, Col = position.Col };
            shots.Add(shot);

            var cellState = targetBoard[position.Row, position.Col];
            ShotResult shotOutcome;

            if (cellState == CellState.Ship)
            {
                targetBoard[position.Row, position.Col] = CellState.Hit;
                shot.IsHit = true;
                _logger.LogInformation($"Hit at position ({position.Row}, {position.Col})");
                
                if (IsShipDestroyed(targetBoard, position))
                {
                    _logger.LogInformation("Ship destroyed");
                    if (IsAllShipsDestroyed(targetBoard))
                    {
                        game.State = GameState.Finished;
                        game.Winner = playerName;
                        shotOutcome = ShotResult.Win;
                        _logger.LogInformation($"Game finished. Winner: {playerName}");
                        string loserName = playerName == game.CreatorName ? game.JoinerName : game.CreatorName;
                        if (!string.IsNullOrEmpty(loserName))
                        {
                           await AddGameToHistory(game, playerName, loserName);
                        }
                    }
                    else
                    {
                        shotOutcome = ShotResult.Destroyed;
                    }
                }
                else
                {
                    shotOutcome = ShotResult.Hit;
                }
            }
            else if (cellState == CellState.Empty)
            {
                targetBoard[position.Row, position.Col] = CellState.Miss;
                shot.IsHit = false;
                _logger.LogInformation($"Miss at position ({position.Row}, {position.Col})");
                game.CurrentTurn = playerName == game.CreatorName ? game.JoinerName : game.CreatorName;
                _logger.LogInformation($"Turn changed to: {game.CurrentTurn}");
                shotOutcome = ShotResult.Miss;
            }
            else
            {
                _logger.LogError($"Cell ({position.Row},{position.Col}) already processed as {cellState}. This should not happen if 'shots' list is checked correctly.");
                shotOutcome = ShotResult.Error;
            }
            return (game, shotOutcome);
        }

        /// <summary>
        /// Проверяет, уничтожен ли корабль в указанной позиции
        /// </summary>
        /// <param name="board">Игровое поле</param>
        /// <param name="position">Позиция для проверки</param>
        /// <returns>true, если корабль уничтожен</returns>
        private bool IsShipDestroyed(CellState[,] board, Position position)
        {
            var directions = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int, int)>();
            queue.Enqueue((position.Row, position.Col));
            visited.Add((position.Row, position.Col));

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();
                foreach (var (dx, dy) in directions)
                {
                    var newRow = row + dx;
                    var newCol = col + dy;
                    if (newRow >= 0 && newRow < 10 && newCol >= 0 && newCol < 10 &&
                        !visited.Contains((newRow, newCol)))
                    {
                        if (board[newRow, newCol] == CellState.Ship)
                        {
                            return false;
                        }
                        if (board[newRow, newCol] == CellState.Hit)
                        {
                            queue.Enqueue((newRow, newCol));
                            visited.Add((newRow, newCol));
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Проверяет, уничтожены ли все корабли на поле
        /// </summary>
        /// <param name="board">Игровое поле</param>
        /// <returns>true, если все корабли уничтожены</returns>
        private bool IsAllShipsDestroyed(CellState[,] board)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board[i, j] == CellState.Ship)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<GameHistory>> GetPlayerGameHistory(string playerName, int count = 10)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new List<GameHistory>();
            }

            return await _dbContext.GameHistories
                .Where(h => h.PlayerUsername == playerName)
                .OrderByDescending(h => h.GameFinishedAt)
                .Take(count)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<PlayerRanking>> GetLeaderboardAsync(int topN)
        {
            if (topN <= 0) topN = 10; 

            return await _dbContext.PlayerRankings
                .OrderByDescending(r => r.Rating)
                .ThenBy(r => r.Wins) 
                .Take(topN)
                .ToListAsync();
        }
    }
} 