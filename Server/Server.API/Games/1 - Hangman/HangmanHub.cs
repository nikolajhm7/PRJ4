using Microsoft.AspNetCore.SignalR;
using Server.API.Services.Interfaces;
using Server.API.DTO;
using Server.API.Models;
using System.Linq.Expressions;
using NSubstitute;
using Microsoft.AspNetCore.Authorization;

namespace Server.API.Games
{
    [Authorize(Policy = "Guest+")]
    public class HangmanHub : Hub
    {
        private readonly ILogger<HangmanHub> _logger;
        private readonly ILobbyManager _lobbyManager;
        private readonly IRandomPicker _randomPicker;
        private readonly ILogicManager<IHangmanLogic> _logicManager;

        public HangmanHub(ILobbyManager lobbyManager, ILogicManager<IHangmanLogic> logicManager, ILogger<HangmanHub> logger, IRandomPicker randomPicker)
        {
            _logger = logger;
            _lobbyManager = lobbyManager;
            _logicManager = logicManager;
            _randomPicker = randomPicker;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobbyId = _lobbyManager.GetLobbyIdFromUsername(username);
            if (lobbyId != null && _lobbyManager.GetGameStatus(lobbyId) == GameStatus.InGame)
            {
                _lobbyManager.UpdateUserInLobby(user, lobbyId);
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

                var result = CreateLogicIfNotCreated(lobbyId, out var wordLength);
                
                if (!result)
                {
                    if (_logicManager.TryGetValue(lobbyId, out var logic))
                    {
                        wordLength = logic.SecretWord.Length;
                    }
                }
                

                await Clients.Caller.SendAsync("GameStarted", wordLength);
                await base.OnConnectedAsync();
            }
            else
            {
                Context.Abort();
            }

        }

        private bool CreateLogicIfNotCreated(string lobbyId, out int length)
        {
            if (_logicManager.LobbyExists(lobbyId))
            {
                var logic = new HangmanLogic(_randomPicker);
                _logicManager.Add(lobbyId, logic);

                length = logic.StartGame();
                return true;
            }
            length = 0;
            return false;
        }

        //public async Task<ActionResult> StartGame(string lobbyId)
        //{
        //    if (_logicManager.LobbyExists(lobbyId))
        //    {
        //        return new(false, "Game lobby already exists, and started.");
        //    }

        //    var logic = new HangmanLogic(_randomPicker);
        //    _logicManager.Add(lobbyId, logic);

        //    var wordLength = logic.StartGame();
        //    await Clients.Group(lobbyId).SendAsync("GameStarted", wordLength);
        //    return new(true, null);
        //}

        public async Task<ActionResult> GuessLetter(string lobbyId, char letter)
        {
            if (_logicManager.TryGetValue(lobbyId, out var logic))
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
            if (_logicManager.TryGetValue(lobbyId, out var logic))
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

            var lobbyId = _lobbyManager.GetLobbyIdFromUsername(username);
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
            if (_lobbyManager.IsHost(user.Username, lobbyId))
            {
                await Clients.Group(lobbyId).SendAsync("LobbyClosed");

                foreach (var member in _lobbyManager.GetUsersInLobby(lobbyId))
                {
                    await Groups.RemoveFromGroupAsync(member.ConnectionId, lobbyId);
                }

                _logicManager.Remove(lobbyId);
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
