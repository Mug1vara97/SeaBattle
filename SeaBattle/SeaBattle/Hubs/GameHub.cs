using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SeaBattle.Models;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SeaBattle.Hubs
{
    public class GameHub : Hub
    {
        private static readonly ConcurrentDictionary<string, GameState> _games = new();
        private readonly ILogger<GameHub> _logger;

        public GameHub(ILogger<GameHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("ReceiveMessage", "Подключено к серверу");
        }

        public async Task<string> CreateGame(string playerName)
        {
            _logger.LogInformation($"Creating new game for player: {playerName}");
            var connectionId = Context.ConnectionId;
            var game = new GameState
            {
                Player1 = new Player { ConnectionId = connectionId, Name = playerName }
            };

            if (string.IsNullOrEmpty(game.GameId))
            {
                _logger.LogError("Failed to generate GameId");
                await Clients.Caller.SendAsync("Error", "Ошибка при создании игры");
                return null;
            }

            if (!_games.TryAdd(game.GameId, game))
            {
                _logger.LogError($"Failed to add game with ID: {game.GameId}");
                await Clients.Caller.SendAsync("Error", "Ошибка при создании игры");
                return null;
            }

            await Groups.AddToGroupAsync(connectionId, game.GameId);
            _logger.LogInformation($"Game created with ID: {game.GameId}");
            await Clients.Caller.SendAsync("GameCreated", game.GameId);
            return game.GameId;
        }

        public async Task JoinGame(string playerName, string gameId)
        {
            _logger.LogInformation($"Player {playerName} attempting to join game: {gameId}");
            if (!_games.TryGetValue(gameId, out var game))
            {
                _logger.LogWarning($"Game not found: {gameId}");
                await Clients.Caller.SendAsync("Error", "Игра не найдена");
                return;
            }

            if (game.Player2 != null)
            {
                _logger.LogWarning($"Game {gameId} is already full");
                await Clients.Caller.SendAsync("Error", "Игра уже заполнена");
                return;
            }

            var connectionId = Context.ConnectionId;
            game.Player2 = new Player { ConnectionId = connectionId, Name = playerName };
            await Groups.AddToGroupAsync(connectionId, gameId);
            _logger.LogInformation($"Player {playerName} joined game {gameId}");
            await Clients.Group(gameId).SendAsync("PlayerJoined", game.Player2.Name);
        }

        public async Task PlaceShips(string gameId, int[] ships)
        {
            _logger.LogInformation($"Attempting to place ships for game: {gameId}");
            if (string.IsNullOrEmpty(gameId))
            {
                _logger.LogError("Game ID is null or empty");
                throw new ArgumentNullException(nameof(gameId));
            }

            if (!_games.TryGetValue(gameId, out var game))
            {
                _logger.LogWarning($"Game not found: {gameId}");
                return;
            }

            var player = game.Player1.ConnectionId == Context.ConnectionId ? game.Player1 : game.Player2;
            if (player == null)
            {
                _logger.LogWarning($"Player not found in game: {gameId}");
                return;
            }

            int[,] board = new int[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board[i, j] = ships[i * 10 + j];
                }
            }

            if (!ValidateShipsPlacement(board))
            {
                _logger.LogWarning($"Invalid ships placement for game: {gameId}");
                await Clients.Caller.SendAsync("InvalidShipsPlacement");
                return;
            }

            player.Board = board;
            player.IsReady = true;
            _logger.LogInformation($"Ships placed successfully for game: {gameId}");

            if (game.Player1?.IsReady == true && game.Player2?.IsReady == true)
            {
                game.GameStarted = true;
                game.IsPlayer1Turn = true;
                _logger.LogInformation($"Game {gameId} started");
                await Clients.Group(gameId).SendAsync("GameStarted");
                await Clients.Client(game.Player1.ConnectionId).SendAsync("TurnChanged", true);
                await Clients.Client(game.Player2.ConnectionId).SendAsync("TurnChanged", true);
            }
            else
            {
                await Clients.Group(gameId).SendAsync("PlayerReady", player.Name);
            }
        }

        private bool ValidateShipsPlacement(int[,] ships)
        {
            int[] shipCounts = new int[5];
            bool[,] visited = new bool[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (ships[i, j] == 1 && !visited[i, j])
                    {
                        int length = 1;
                        visited[i, j] = true;

                        int k = j + 1;
                        while (k < 10 && ships[i, k] == 1)
                        {
                            visited[i, k] = true;
                            length++;
                            k++;
                        }

                        if (length == 1)
                        {
                            k = i + 1;
                            while (k < 10 && ships[k, j] == 1)
                            {
                                visited[k, j] = true;
                                length++;
                                k++;
                            }
                        }

                        if (length > 4 || length < 1) return false;
                        shipCounts[length]++;
                    }
                }
            }

            return shipCounts[1] == 4 && shipCounts[2] == 3 && shipCounts[3] == 2 && shipCounts[4] == 1;
        }

        public async Task MakeShot(string gameId, int row, int col)
        {
            if (!_games.TryGetValue(gameId, out var game)) return;
            if (!game.GameStarted) return;

            var currentPlayer = game.IsPlayer1Turn ? game.Player1 : game.Player2;
            var opponent = game.IsPlayer1Turn ? game.Player2 : game.Player1;

            if (currentPlayer.ConnectionId != Context.ConnectionId) return;

            var result = ProcessShot(opponent.Board, row, col);
            
            await Clients.Caller.SendAsync("ReceiveShot", row, col, result);
            
            await Clients.Client(opponent.ConnectionId).SendAsync("ReceiveHit", row, col, result);

            if (result == "hit")
            {
                if (CheckWin(opponent.Board))
                {
                    game.Winner = currentPlayer.Name;
                    await Clients.Group(gameId).SendAsync("GameOver", currentPlayer.Name);
                }
            }
            else
            {
                game.IsPlayer1Turn = !game.IsPlayer1Turn;
                await Clients.Client(game.Player1.ConnectionId).SendAsync("TurnChanged", game.IsPlayer1Turn);
                await Clients.Client(game.Player2.ConnectionId).SendAsync("TurnChanged", game.IsPlayer1Turn);
            }
        }

        private string ProcessShot(int[,] board, int row, int col)
        {
            if (row < 0 || row >= 10 || col < 0 || col >= 10) return "invalid";
            
            if (board[row, col] == 1)
            {
                board[row, col] = 2;
                return "hit";
            }
            
            if (board[row, col] == 0)
            {
                board[row, col] = 3;
                return "miss";
            }
            
            return "invalid";
        }

        private bool CheckWin(int[,] board)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board[i, j] == 1) return false;
                }
            }
            return true;
        }
    }
} 