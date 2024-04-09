using Microsoft.AspNetCore.SignalR.Client;
using Client.UI.DTO;
using Microsoft.Extensions.Configuration;

namespace Client.UI.Services
{
    public class LobbyService : ConnectionService
    {

        public event Action<ConnectedUserDTO>? UserJoinedLobbyEvent;
        public event Action<ConnectedUserDTO>? UserLeftLobbyEvent;
        public event Action? GameStartedEvent;
        public event Action? LobbyClosedEvent;

        public LobbyService(IConfiguration configuration) : base(configuration["ConnectionSettings:ApiUrl"] + configuration["ConnectionSettings:LobbyEndpoint"])
        {
            On<ConnectedUserDTO>("UserJoinedLobby", (user) =>
                UserJoinedLobbyEvent?.Invoke(user));

            On<ConnectedUserDTO>("UserLeftLobby", (user) =>
                UserLeftLobbyEvent?.Invoke(user));

            On("GameStarted", () =>
                GameStartedEvent?.Invoke());

            On("LobbyClosed", () =>
                LobbyClosedEvent?.Invoke());
        }

        public async Task<ActionResult> CreateLobbyAsync()
        {
                return await InvokeAsync("CreateLobby");
        }

        public async Task<ActionResult> JoinLobbyAsync(string lobbyId)
        {
            return await InvokeAsync("JoinLobby", lobbyId);

        }

        public async Task<ActionResult> LeaveLobbyAsync(string lobbyId)
        {
            return await InvokeAsync("LeaveLobby", lobbyId);
        }

        public async Task<ActionResult> StartGameAsync(string lobbyId)
        {
            return await InvokeAsync("StartGame", lobbyId);
        }
    }
}
