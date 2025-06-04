using Microsoft.AspNetCore.SignalR;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Hubs
{
    /// <summary>
    /// SignalR хаб для управления игровыми лобби
    /// </summary>
    public class LobbyHub : Hub
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Инициализирует новый экземпляр хаба лобби
        /// </summary>
        /// <param name="gameService">Сервис для управления игровым процессом</param>
        public LobbyHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Обрабатывает подключение клиента к хабу и отправляет список открытых лобби
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        /// <summary>
        /// Обрабатывает отключение клиента от хаба и обновляет список открытых лобби
        /// </summary>
        /// <param name="exception">Исключение, если оно возникло при отключении</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        /// <summary>
        /// Создает новое лобби для игры
        /// </summary>
        /// <param name="creatorName">Имя создателя лобби</param>
        /// <param name="isOpenLobby">Флаг, указывающий является ли лобби открытым</param>
        public async Task CreateLobby(string creatorName, bool isOpenLobby)
        {
            var game = await _gameService.CreateGame(creatorName, isOpenLobby);
            await Clients.All.SendAsync("LobbiesUpdated", await _gameService.GetOpenLobbies());
        }

        /// <summary>
        /// Присоединяет игрока к существующему лобби
        /// </summary>
        /// <param name="gameId">Идентификатор игры</param>
        /// <param name="opponentName">Имя присоединяющегося игрока</param>
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