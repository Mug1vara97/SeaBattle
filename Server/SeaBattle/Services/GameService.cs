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
    public class GameService : IGameService
    {
        private static readonly ConcurrentDictionary<string, Game> _games = new();
        private readonly ILogger<GameService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private const int InitialRating = 1000;
        private const int RatingChangeOnWin = 15;
        private const int RatingChangeOnLoss = 10;

        public GameService(ILogger<GameService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

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

        public Task<Game?> GetGame(string gameId)
        {
            return Task.FromResult(_games.TryGetValue(gameId, out var game) ? game : null);
        }

        public Task<List<Game>> GetOpenLobbies()
        {
            return Task.FromResult(_games.Values
                .Where(g => g.IsOpenLobby && g.State == GameState.WaitingForOpponent)
                .ToList());
        }

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