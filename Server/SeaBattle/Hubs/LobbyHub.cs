using Microsoft.AspNetCore.SignalR;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly IGameService _gameService;

        public LobbyHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        public async Task CreateLobby(string creatorName, bool isOpenLobby)
        {
            var game = await _gameService.CreateGame(creatorName, isOpenLobby);
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        public async Task JoinLobby(string gameId, string opponentName)
        {
            var game = await _gameService.JoinGame(gameId, opponentName);
            if (game != null)
            {
                await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
            }
        }
    }
} 