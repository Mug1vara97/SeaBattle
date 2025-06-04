using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SeaBattle.Models;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using SeaBattle.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace SeaBattle.Hubs
{
    /// <summary>
    /// SignalR хаб для обработки real-time взаимодействия в игре морской бой
    /// </summary>
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameHub> _logger;
        private static readonly ConcurrentDictionary<string, (string GameId, string PlayerName)> _playerConnections = new();

        /// <summary>
        /// Инициализирует новый экземпляр хаба игры
        /// </summary>
        /// <param name="gameService">Сервис для управления игровым процессом</param>
        /// <param name="logger">Логгер для записи событий</param>
        public GameHub(IGameService gameService, ILogger<GameHub> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает подключение клиента к хабу
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogInformation($"Client connected: {Context.ConnectionId}");
                await base.OnConnectedAsync();
                await Clients.Caller.SendAsync("ReceiveMessage", "Подключено к серверу");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        /// <summary>
        /// Обрабатывает отключение клиента от хаба
        /// </summary>
        /// <param name="exception">Исключение, если оно возникло при отключении</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (_playerConnections.TryRemove(Context.ConnectionId, out var connectionInfo))
                {
                    _logger.LogInformation($"Player {connectionInfo.PlayerName} (Connection: {Context.ConnectionId}) disconnected from game {connectionInfo.GameId}.");
                    await Clients.Group(connectionInfo.GameId).SendAsync("PlayerDisconnected", connectionInfo.PlayerName);
                }
                else
                {
                    _logger.LogInformation($"Client disconnected: {Context.ConnectionId}. No specific game association found to remove.");
                }
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
                throw;
            }
        }

        /// <summary>
        /// Получает список открытых лобби
        /// </summary>
        /// <returns>Список доступных игр</returns>
        public async Task<List<Game>> GetOpenLobbies()
        {
            try
            {
                _logger.LogInformation($"Getting open lobbies for client: {Context.ConnectionId}");
                var lobbies = await _gameService.GetOpenLobbies();
                await Clients.Caller.SendAsync("LobbiesUpdated", lobbies);
                return lobbies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting open lobbies");
                await Clients.Caller.SendAsync("Error", "Ошибка при получении списка лобби: " + ex.Message);
                return new List<Game>();
            }
        }

        /// <summary>
        /// Создает новую игру
        /// </summary>
        /// <param name="creatorName">Имя создателя игры</param>
        /// <param name="isOpenLobby">Флаг, указывающий является ли лобби открытым</param>
        /// <returns>Идентификатор созданной игры</returns>
        public async Task<string> CreateGame(string creatorName, bool isOpenLobby = false)
        {
            try
            {
                _logger.LogInformation($"Creating game for creator: {creatorName}, isOpenLobby: {isOpenLobby}");
                var game = await _gameService.CreateGame(creatorName, isOpenLobby);
                
                if (game == null)
                {
                    _logger.LogError("GameService.CreateGame returned null.");
                    throw new Exception("Failed to create game object.");
                }

                _playerConnections[Context.ConnectionId] = (game.Id, creatorName);
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                await Clients.Caller.SendAsync("GameCreated", game.Id);
                
                var openLobbies = await _gameService.GetOpenLobbies();
                await Clients.All.SendAsync("LobbiesUpdated", openLobbies);
                
                _logger.LogInformation($"Game created successfully: {game.Id} by {creatorName}. Connection: {Context.ConnectionId}");
                return game.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating game for {creatorName}");
                await Clients.Caller.SendAsync("Error", "Ошибка при создании игры: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Присоединяет игрока к существующей игре
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="joinerName">Имя присоединяющегося игрока</param>
        /// <returns>Информация об игре или null, если присоединиться не удалось</returns>
        public async Task<Game?> JoinGame(string gameId, string joinerName)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId)) throw new ArgumentNullException(nameof(gameId));
                if (string.IsNullOrEmpty(joinerName)) throw new ArgumentNullException(nameof(joinerName));

                _logger.LogInformation($"Player {joinerName} attempting to join game: {gameId}");
                var game = await _gameService.JoinGame(gameId, joinerName);
                
                if (game == null)
                {
                    _logger.LogWarning($"Failed to join game {gameId} for player {joinerName}. Game service returned null or conditions not met.");
                    await Clients.Caller.SendAsync("Error", "Не удалось присоединиться к игре. Возможно, игра уже занята или не существует.");
                    return null;
                }

                _playerConnections[Context.ConnectionId] = (game.Id, joinerName);
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                
                _logger.LogInformation($"Player {joinerName} (C:{Context.ConnectionId}) joined game: {game.Id}. Notifying players.");

                await SendPersonalizedDataToGroup(game, "GameUpdated", CreateBasePersonalizedDto);
                
                await SendPersonalizedDataToGroup(game, "SecondPlayerJoined", CreateBasePersonalizedDto); 
                
                var openLobbies = await _gameService.GetOpenLobbies();
                await Clients.All.SendAsync("LobbiesUpdated", openLobbies);
                
                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining game {gameId} for player {joinerName}");
                await Clients.Caller.SendAsync("Error", "Ошибка при присоединении к игре: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Отправляет персонализированные данные группе игроков
        /// </summary>
        private async Task SendPersonalizedDataToGroup(Game game, string eventName, Func<Game, string, object> dtoFactory)
        {
            if (game == null) return;

            var gameConnections = _playerConnections
                .Where(pc => pc.Value.GameId == game.Id)
                .ToList();

            foreach (var connEntry in gameConnections)
            {
                var connectionId = connEntry.Key;
                var playerNameForThisConnection = connEntry.Value.PlayerName;

                if (string.IsNullOrEmpty(playerNameForThisConnection))
                {
                    _logger.LogWarning($"Found connection {connectionId} for game {game.Id} with no player name.");
                    continue;
                }
                
                var personalizedDto = dtoFactory(game, playerNameForThisConnection);
                await Clients.Client(connectionId).SendAsync(eventName, personalizedDto);
            }
        }
        
        /// <summary>
        /// Создает базовый DTO с персонализированными данными для игрока
        /// </summary>
        private object CreateBasePersonalizedDto(Game game, string playerNameForThisConnection)
        {
            bool isPlayerTheCreator = playerNameForThisConnection == game.CreatorName;
            List<Position> myShotsList;
            List<Position> opponentShotsHitList;

            if (isPlayerTheCreator)
            {
                myShotsList = game.CreatorShots ?? new List<Position>();
                opponentShotsHitList = game.JoinerShots ?? new List<Position>();
            }
            else
            {
                myShotsList = game.JoinerShots ?? new List<Position>();
                opponentShotsHitList = game.CreatorShots ?? new List<Position>();
            }

            return new {
                id = game.Id,
                creatorName = game.CreatorName,
                joinerName = game.JoinerName,
                creatorBoard = game.CreatorBoard,
                joinerBoard = game.JoinerBoard,
                state = game.State,
                currentTurn = game.CurrentTurn,
                winner = game.Winner,
                isOpenLobby = game.IsOpenLobby,
                creatorReady = game.CreatorReady,
                joinerReady = game.JoinerReady,
                creatorBoardSet = game.CreatorBoardSet,
                joinerBoardSet = game.JoinerBoardSet,
                isCreator = isPlayerTheCreator,
                myShots = myShotsList,
                opponentShots = opponentShotsHitList
            };
        }

        /// <summary>
        /// Устанавливает готовность игрока к началу игры
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        /// <returns>Обновленная информация об игре или null в случае ошибки</returns>
        public async Task<Game?> SetReady(string gameId, string playerName)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId)) throw new ArgumentNullException(nameof(gameId));
                if (string.IsNullOrEmpty(playerName)) throw new ArgumentNullException(nameof(playerName));

                _logger.LogInformation($"Player {playerName} setting ready for game: {gameId}");
                var game = await _gameService.SetReady(gameId, playerName);
                
                if (game == null)
                {
                     _logger.LogWarning($"Failed to set ready for P:{playerName}, G:{gameId}. Game service returned null.");
                    throw new Exception("Failed to set ready status, game not found or player invalid.");
                }
                
                _logger.LogInformation($"P:{playerName} is ready for G:{game.Id}. Current State: {game.State}. Notifying group.");
                await SendPersonalizedDataToGroup(game, "GameUpdated", CreateBasePersonalizedDto);
                
                if (game.State == GameState.InProgress)
                {
                    _logger.LogInformation($"Game {game.Id} started. Sending GameStarted and TurnChanged. First turn: {game.CurrentTurn}");
                    await SendPersonalizedDataToGroup(game, "GameStarted", CreateBasePersonalizedDto);
                    if (!string.IsNullOrEmpty(game.CurrentTurn))
                    {
                        await Clients.Group(game.Id).SendAsync("TurnChanged", game.CurrentTurn);
                    }
                }
                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SetReady for G:{gameId}, P:{playerName}");
                await Clients.Caller.SendAsync("Error", "Ошибка при установке статуса готовности: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Выполняет выстрел по указанной позиции
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока, выполняющего выстрел</param>
        /// <param name="position">Позиция выстрела</param>
        /// <returns>Результат выстрела</returns>
        public async Task<ShotResultResponse> MakeShot(string gameId, string playerName, Position position)
        {
            var serviceResult = await _gameService.MakeShot(gameId, playerName, position);
            
            if (serviceResult.game != null)
            {
                _logger.LogInformation($"Shot by P:{playerName} in G:{gameId} at ({position.Row},{position.Col}). Result: {serviceResult.result}. Notifying players.");
                await SendPersonalizedDataToGroup(serviceResult.game, "GameUpdated", CreateBasePersonalizedDto);

                await SendPersonalizedDataToGroup(serviceResult.game, "ShotResult", (g, pNameToPersonalizeFor) => {
                    var baseDto = (dynamic)CreateBasePersonalizedDto(g, pNameToPersonalizeFor);
                    return new {
                        id = baseDto.id,
                        creatorName = baseDto.creatorName,
                        joinerName = baseDto.joinerName,
                        creatorBoard = baseDto.creatorBoard,
                        joinerBoard = baseDto.joinerBoard,
                        state = baseDto.state,
                        currentTurn = baseDto.currentTurn,
                        winner = baseDto.winner,
                        isOpenLobby = baseDto.isOpenLobby,
                        creatorReady = baseDto.creatorReady,
                        joinerReady = baseDto.joinerReady,
                        myShots = baseDto.myShots,
                        opponentShots = baseDto.opponentShots,
                        isCreator = baseDto.isCreator,
                        shooter = playerName,
                        position = position,
                        result = serviceResult.result,
                        isHit = serviceResult.result == Models.ShotResult.Hit || 
                                serviceResult.result == Models.ShotResult.Destroyed || 
                                serviceResult.result == Models.ShotResult.Win
                    };
                });

                if (serviceResult.result == Models.ShotResult.Win || serviceResult.result == Models.ShotResult.Destroyed && serviceResult.game.State == GameState.Finished)
                {
                    _logger.LogInformation($"Game {gameId} ended due to shot. Winner: {serviceResult.game.Winner}. Sending GameEnded.");
                    await SendPersonalizedDataToGroup(serviceResult.game, "GameEnded", CreateBasePersonalizedDto);
                }
                else if (serviceResult.result == Models.ShotResult.Miss)
                {
                     _logger.LogInformation($"Turn changed in G:{gameId} to {serviceResult.game.CurrentTurn}. Sending TurnChanged.");
                    await Clients.Group(gameId).SendAsync("TurnChanged", serviceResult.game.CurrentTurn);
                }
            }
            else
            {
                 _logger.LogWarning($"MakeShot for G:{gameId} by P:{playerName} returned null game from service. Result: {serviceResult.result}");
            }

            return new ShotResultResponse
            {
                Result = serviceResult.result,
                Position = position,
                IsHit = serviceResult.result == Models.ShotResult.Hit || 
                        serviceResult.result == Models.ShotResult.Destroyed || 
                        serviceResult.result == Models.ShotResult.Win,
                CurrentTurn = serviceResult.game?.CurrentTurn,
                GameState = serviceResult.game?.State ?? GameState.Unknown
            };
        }

        /// <summary>
        /// Отправляет расстановку кораблей на игровом поле
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="clientBoard">Массив с расстановкой кораблей</param>
        /// <returns>Обновленная информация об игре или null в случае ошибки</returns>
        public async Task<Game?> SubmitBoardPlacement(string gameId, string playerName, int[][] clientBoard)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId)) throw new ArgumentNullException(nameof(gameId));
                if (string.IsNullOrEmpty(playerName)) throw new ArgumentNullException(nameof(playerName));
                if (clientBoard == null || clientBoard.Length != 10 || clientBoard.Any(row => row.Length != 10))
                {
                    _logger.LogError($"SubmitBoardPlacement: Invalid board data received for G:{gameId}, P:{playerName}.");
                    await Clients.Caller.SendAsync("Error", "Некорректные данные доски.");
                    return null;
                }

                _logger.LogInformation($"Player {playerName} attempting to submit board for game: {gameId}");

                var serverBoard = new CellState[10, 10];
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        serverBoard[i, j] = (CellState)clientBoard[i][j]; 
                    }
                }
                
                var game = await _gameService.PlaceShipsAsync(gameId, playerName, serverBoard);

                if (game == null)
                {
                    _logger.LogWarning($"Failed to place ships for P:{playerName}, G:{gameId}. Game service returned null or error.");
                    await Clients.Caller.SendAsync("Error", "Не удалось разместить корабли. Возможно, неверная расстановка или игра не найдена.");
                    var currentGame = await _gameService.GetGame(gameId);
                    if (currentGame != null)
                    {
                         await SendPersonalizedDataToGroup(currentGame, "GameUpdated", CreateBasePersonalizedDto);
                    }
                    return null;
                }

                _logger.LogInformation($"P:{playerName} successfully placed ships for G:{game.Id}. Notifying group.");
                await SendPersonalizedDataToGroup(game, "GameUpdated", CreateBasePersonalizedDto);

                if (game.CreatorBoardSet && game.JoinerBoardSet && game.State == GameState.WaitingForReady)
                {
                    _logger.LogInformation($"Both players in game {game.Id} have placed ships. Ready for SetReady state.");
                }
                 else if (game.CreatorBoardSet && game.JoinerName == null)
                {
                     _logger.LogInformation($"Creator in game {game.Id} has placed ships. Waiting for opponent.");
                }
                else if ((game.CreatorBoardSet && !game.JoinerBoardSet && game.JoinerName != null) || 
                         (!game.CreatorBoardSet && game.JoinerBoardSet && game.CreatorName != null))
                {
                    _logger.LogInformation($"One player in game {game.Id} has placed ships. Waiting for the other.");
                }

                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SubmitBoardPlacement for G:{gameId}, P:{playerName}");
                await Clients.Caller.SendAsync("Error", "Ошибка на сервере при размещении кораблей: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Получает текущее состояние игры
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="playerName">Имя игрока</param>
        public async Task GetGameState(string gameId, string playerName)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId)) throw new ArgumentNullException(nameof(gameId));
                if (string.IsNullOrEmpty(playerName)) throw new ArgumentNullException(nameof(playerName));

                _logger.LogInformation($"P:{playerName} (C:{Context.ConnectionId}) requesting state for G:{gameId}.");
                var game = await _gameService.GetGame(gameId);

                if (game == null)
                {
                    _logger.LogWarning($"G:{gameId} not found for P:{playerName} in GetGameState.");
                    await Clients.Caller.SendAsync("Error", $"Игра с ID {gameId} не найдена.");
                    return;
                }

                if (!_playerConnections.ContainsKey(Context.ConnectionId) || 
                    _playerConnections[Context.ConnectionId].GameId != gameId || 
                    _playerConnections[Context.ConnectionId].PlayerName != playerName)
                {
                    _playerConnections[Context.ConnectionId] = (gameId, playerName);
                    await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
                    _logger.LogInformation($"P:{playerName} (C:{Context.ConnectionId}) re-registered for G:{gameId} via GetGameState.");
                }
                
                var personalizedDto = CreateBasePersonalizedDto(game, playerName);
                _logger.LogInformation($"Sending GameState to P:{playerName} (C:{Context.ConnectionId}) for G:{gameId}. IsCreator: {((dynamic)personalizedDto).isCreator}");
                await Clients.Caller.SendAsync("GameState", personalizedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetGameState for G:{gameId}, P:{playerName}");
                await Clients.Caller.SendAsync("Error", "Ошибка при получении состояния игры: " + ex.Message);
            }
        }

        /// <summary>
        /// Получает историю игр пользователя
        /// </summary>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="count">Количество записей для отображения</param>
        public async Task GetMyGameHistory(string playerName, int count = 10)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                _logger.LogWarning("GetMyGameHistory: Player name not provided by client.");
                await Clients.Caller.SendAsync("Error", "Не удалось определить пользователя для запроса истории игр (имя не передано).");
                await Clients.Caller.SendAsync("ReceiveGameHistory", new List<GameHistory>());
                return;
            }

            _logger.LogInformation($"Player {playerName} (C:{Context.ConnectionId}) requesting their game history (last {count}).");

            try
            {
                var history = await _gameService.GetPlayerGameHistory(playerName, count);
                await Clients.Caller.SendAsync("ReceiveGameHistory", history);
                _logger.LogInformation($"Sent {history.Count} game history records to player {playerName}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting game history for player {playerName}.");
                await Clients.Caller.SendAsync("Error", "Ошибка при получении истории игр.");
                await Clients.Caller.SendAsync("ReceiveGameHistory", new List<GameHistory>());
            }
        }
    }
} 