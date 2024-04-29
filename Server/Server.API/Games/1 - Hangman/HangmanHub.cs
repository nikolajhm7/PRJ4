using Microsoft.AspNetCore.SignalR;
using Server.API.Services.Interfaces;
using Server.API.DTO;
using Server.API.Models;
using System.Linq.Expressions;
using NSubstitute;

namespace Server.API.Games
{
    public class HangmanHub : Hub
    {
        private readonly ILogger<HangmanHub> _logger;
        private readonly ILobbyManager _lobbyManager;
        public readonly Dictionary<string, HangmanLogic> LobbyLogic = [];
        private readonly IRandomPicker _randomPicker;

        public HangmanHub(ILobbyManager lobbyManager, ILogger<HangmanHub> logger, IRandomPicker randomPicker)
        {
            _logger = logger;
            _lobbyManager = lobbyManager;
            _randomPicker = randomPicker;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobbyId = _lobbyManager.GetLobbyIdFromUser(user);
            if (lobbyId != null && _lobbyManager.GetGameStatus(lobbyId) == GameStatus.InGame)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                await base.OnConnectedAsync();
            }
            else
            {
                Context.Abort();
            }

        }

        public async Task<ActionResult> StartGame(string lobbyId)
        {
            if (LobbyLogic.ContainsKey(lobbyId))
            {
                return new(false, "Game lobby already exists, and started.");
            }

            var logic = new HangmanLogic(_randomPicker);
            LobbyLogic.Add(lobbyId, logic);

            var wordLength = logic.StartGame();
            await Clients.Group(lobbyId).SendAsync("GameStarted", wordLength);
            return new(true, null);
        }

        public async Task<ActionResult> GuessLetter(string lobbyId, char letter)
        {
            if (LobbyLogic.TryGetValue(lobbyId, out var logic))
            {
                List<int> positions;
                var isCorrect = logic.GuessLetter(letter, out positions);
                await Clients.Group(lobbyId).SendAsync("GuessResult", letter, isCorrect, positions);

                var res = logic.IsGameOver();
                if (res)
                {
                    await Clients.Group(lobbyId).SendAsync("GameOver", logic.DidUserWin(), logic.SecretWord);
                }

                return new(true, null);
            }
            return new(false, "Lobby does not exist.");
        }

        public async Task<ActionResult> RestartGame(string lobbyId)
        {
            if (LobbyLogic.TryGetValue(lobbyId, out var logic))
            {
                var wordLength = logic.StartGame();
                await Clients.Group(lobbyId).SendAsync("GameStarted", wordLength);
                return new(true, null);
            }
            return new(false, "Lobby does not exist.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Handling disconnect of user {UserName}.", Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobbyId = _lobbyManager.GetLobbyIdFromUser(user);
            if (lobbyId != null)
            {
                await RemoveUserFromLobby(lobbyId, user);
            }

            _logger.LogDebug("Successfully disconnected {UserName}.", username);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task RemoveUserFromLobby(string lobbyId, ConnectedUserDTO user)
        {
            // If host disconnects, close lobby and remove all members.
            if (_lobbyManager.IsHost(Context.ConnectionId, lobbyId))
            {
                await Clients.Group(lobbyId).SendAsync("LobbyClosed");

                foreach (var member in _lobbyManager.GetUsersInLobby(lobbyId))
                {
                    await Groups.RemoveFromGroupAsync(member.ConnectionId, lobbyId);
                }

                _lobbyManager.RemoveLobby(lobbyId);
            }
            else
            {
                await Clients.Group(lobbyId).SendAsync("UserLeftLobby", user.Username);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                _lobbyManager.RemoveFromLobby(user, lobbyId);
                _logger.LogInformation("{UserName} successfully left lobby {LobbyId}.", user.Username, lobbyId);
            }
        }
    }
}
