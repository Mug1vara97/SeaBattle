using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle.Models
{
    public enum GameState
    {
        WaitingForOpponent,
        WaitingForReady,
        InProgress,
        Finished,
        Unknown
    }

    public class GameStateInfo
    {
        public required string GameId { get; set; }
        public required string CreatorName { get; set; }
        public string? OpponentName { get; set; }
        public bool IsCreatorReady { get; set; }
        public bool IsOpponentReady { get; set; }
        public bool IsCreatorTurn { get; set; }
        public bool IsGameStarted { get; set; }
        public bool IsGameEnded { get; set; }
        public string? Winner { get; set; }
        public bool IsOpenLobby { get; set; }
        public DateTime CreatedAt { get; set; }
        public required CellState[,] CreatorBoard { get; set; }
        public required CellState[,] OpponentBoard { get; set; }
        public required Position[] CreatorShots { get; set; }
        public required Position[] OpponentShots { get; set; }
    }

    public static class GameStateManager
    {
        private static readonly ConcurrentDictionary<string, GameStateInfo> _games = new();

        public static GameStateInfo CreateGame(string creatorName, bool isOpenLobby = false)
        {
            var gameId = Guid.NewGuid().ToString();
            var game = new GameStateInfo
            {
                GameId = gameId,
                CreatorName = creatorName,
                IsCreatorReady = false,
                IsOpponentReady = false,
                IsCreatorTurn = true,
                IsGameStarted = false,
                IsGameEnded = false,
                IsOpenLobby = isOpenLobby,
                CreatedAt = DateTime.UtcNow,
                CreatorBoard = new CellState[10, 10],
                OpponentBoard = new CellState[10, 10],
                CreatorShots = new Position[10],
                OpponentShots = new Position[10]
            };

            _games.TryAdd(gameId, game);
            return game;
        }

        public static GameStateInfo? GetGame(string gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return game;
        }

        public static IEnumerable<GameStateInfo> GetOpenLobbies()
        {
            return _games.Values
                .Where(g => g.IsOpenLobby && !g.IsGameStarted && g.OpponentName == null)
                .OrderByDescending(g => g.CreatedAt);
        }

        public static void RemoveGame(string gameId)
        {
            _games.TryRemove(gameId, out _);
        }

        public enum GameState
        {
            WaitingForOpponent = 0,
            WaitingForReady = 1,
            InProgress = 2,
            Finished = 3
        }
    }
} 