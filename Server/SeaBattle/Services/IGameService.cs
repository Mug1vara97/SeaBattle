using SeaBattle.Models;

namespace SeaBattle.Services
{
    public interface IGameService
    {
        Task<Game> CreateGame(string creatorName, bool isOpenLobby);
        Task<Game?> JoinGame(string gameId, string joinerName);
        Task<Game?> SetReady(string gameId, string playerName);
        Task<Game?> GetGame(string gameId);
        Task<List<Game>> GetOpenLobbies();
        Task<(Game? game, ShotResult result)> MakeShot(string gameId, string playerName, Position position);
        Task<List<GameHistory>> GetPlayerGameHistory(string playerName, int count = 10);
        Task<List<PlayerRanking>> GetLeaderboardAsync(int topN);
        Task<Game?> PlaceShipsAsync(string gameId, string playerName, CellState[,] clientBoard);
    }
} 