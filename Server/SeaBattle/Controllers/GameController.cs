using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SeaBattle.Hubs;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IHubContext<LobbyHub> _lobbyHub;

        public GameController(IGameService gameService, IHubContext<LobbyHub> lobbyHub)
        {
            _gameService = gameService;
            _lobbyHub = lobbyHub;
        }

        [HttpPost("create")]
        public async Task<ActionResult<Game>> CreateGame([FromBody] CreateGameRequest request)
        {
            var game = await _gameService.CreateGame(request.CreatorName, request.IsOpenLobby);
            await _lobbyHub.Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
            return Ok(game);
        }

        [HttpGet("lobbies")]
        public async Task<ActionResult<IEnumerable<LobbyInfo>>> GetOpenLobbies()
        {
            var lobbies = await _gameService.GetOpenLobbies();
            return Ok(lobbies);
        }

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

        [HttpGet("leaderboard")]
        public async Task<ActionResult<IEnumerable<PlayerRanking>>> GetLeaderboard([FromQuery] int count = 10)
        {
            var leaderboard = await _gameService.GetLeaderboardAsync(count);
            return Ok(leaderboard);
        }
    }

    public class CreateGameRequest
    {
        public string CreatorName { get; set; } = string.Empty;
        public bool IsOpenLobby { get; set; }
    }

    public class JoinGameRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
    }

    public class ReadyRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
    }

    public class ShotRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public Position Position { get; set; } = new();
    }
} 