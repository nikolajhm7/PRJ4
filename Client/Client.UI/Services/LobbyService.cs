using Microsoft.AspNetCore.SignalR.Client;
using Client.UI.DTO;

namespace Client.UI.Services
{
    public class LobbyService
    {

        private readonly IConnectionService _connectionService;

        public event Action<ConnectedUserDTO>? UserJoinedLobbyEvent;
        public event Action<ConnectedUserDTO>? UserLeftLobbyEvent;
        public event Action? GameStartedEvent;

        public LobbyService(IConnectionService connectionService)
        {
            _connectionService = connectionService;

            _connectionService.On<ConnectedUserDTO>("UserJoinedLobby", (user) =>
                UserJoinedLobbyEvent?.Invoke(user));

            _connectionService.On<ConnectedUserDTO>("UserLeftLobby", (user) =>
                UserLeftLobbyEvent?.Invoke(user));

            _connectionService.On("GameStarted", () =>
                GameStartedEvent?.Invoke());
        }

        public async Task<IConnectionService.ActionResult> CreateLobbyAsync()
        {
                return await _connectionService.InvokeAsync("CreateLobby");
        }

        public async Task<IConnectionService.ActionResult> JoinLobbyAsync(string lobbyId)
        {
            return await _connectionService.InvokeAsync("JoinLobby", lobbyId);

        }

        public async Task<IConnectionService.ActionResult> LeaveLobbyAsync(string lobbyId)
        {
            return await _connectionService.InvokeAsync("LeaveLobby", lobbyId);
        }

        public async Task<IConnectionService.ActionResult> StartGameAsync(string lobbyId)
        {
            return await _connectionService.InvokeAsync("StartGame", lobbyId);
        }
    }
}
