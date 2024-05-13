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

                lock (_logicManager)
                {
                    var result = CreateLogicIfNotCreated(lobbyId, out var wordLength);

                    if (!result)
                    {
                        if (_logicManager.TryGetValue(lobbyId, out var logic))
                        {
                            wordLength = logic.SecretWord.Length;
                        }
                    }

                    Clients.Caller.SendAsync("GameStarted", wordLength);
                }
                await base.OnConnectedAsync();
            }
            else
            {
                Context.Abort();
            }

        }

        private bool CreateLogicIfNotCreated(string lobbyId, out int length)
        {
            if (!_logicManager.LobbyExists(lobbyId))
            {
                var logic = new HangmanLogic(_randomPicker);
                _logicManager.Add(lobbyId, logic);

                length = logic.StartGame();
                return true;
            }
            else
            {
                length = 0;
                return false;
            }
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
            var currentUser = Context.User?.Identity?.Name;
            if (_logicManager.TryGetValue(lobbyId, out var logic))
            {
                List<int> positions;
                var userQueue = logic.GetQueue();
                if (userQueue == null)
                {
                    var q = GetQueueForGame(lobbyId);
                    userQueue = q.Result.Value;
                }

                if (currentUser == userQueue.Peek())
                {
                    userQueue.Dequeue();
                    userQueue.Enqueue(currentUser);
                    logic.SetQueue(userQueue);
                    var isCorrect = logic.GuessLetter(letter, out positions);
                    await Clients.Group(lobbyId).SendAsync("GuessResult", letter, isCorrect, positions);

                    var res = logic.IsGameOver();
                    if (res)
                    {
                        await Clients.Group(lobbyId).SendAsync("GameOver", logic.DidUserWin(), logic.SecretWord);
                    }

                    return new(true, null);
                }
                else
                {
                    return new(false, "Not the users turn!");
                }
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
                await RemovePlayerFromQueue(lobbyId, user.Username);
                _logger.LogInformation("{UserName} successfully left lobby {LobbyId}.", user.Username, lobbyId);
            }
        }

        public async Task<ActionResult<List<ConnectedUserDTO>>> GetUsersInGame(string lobbyId)
        {
            _logger.LogDebug("Attempting to get users in game with lobbyid {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new(false, "Authentication context is not available.", []);
            }

            if (_lobbyManager.LobbyExists(lobbyId))
            {
                var users = _lobbyManager.GetUsersInLobby(lobbyId);

                _logger.LogInformation("{UserName} successfully got users in game {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new(true, null, users);
            }
            else
            {
                _logger.LogError("Attempt to get users in non-existing lobby {LobbyId}.", lobbyId);
                return new(false, "Lobby does not exist.", []);
            }
        }

        public async Task<ActionResult<Queue<string>>> GetQueueForGame(string lobbyId)
        {
            _logger.LogDebug("Attempting to get user queue for game with LobbyId {LobbyId}", lobbyId);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new(false, "Authentication context is not available.", []);
            }
            if (_logicManager.TryGetValue(lobbyId, out var logic))
            {
                var users = _lobbyManager.GetUsersInLobby(lobbyId);
                Queue<string> userOrder = [];
                foreach (var user in users)
                {
                    if (!userOrder.Contains(user.Username))
                    {
                        userOrder.Enqueue(user.Username);
                    }
                }
                logic.SetQueue(userOrder);


                _logger.LogInformation("{UserName} successfully got user queue in game {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new(true, null, userOrder);
            }

            else
            {
                _logger.LogError("Attempt to get user queue in non-existing lobby {LobbyId}.", lobbyId);
                return new(false, "Lobby does not exist.", []);
            }
        }

        public async Task<ActionResult> LeaveGame(string lobbyId)
        {
            _logger.LogDebug("Attempting to leave lobby {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            if (_lobbyManager.LobbyExists(lobbyId))
            {
                var user = new ConnectedUserDTO(username, Context.ConnectionId);

                await RemoveUserFromLobby(lobbyId, user);

                return new ActionResult(true, null);
            }
            else
            {
                _logger.LogError("Attempt to leave non-existing lobby {LobbyId}.", lobbyId);
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        public async Task<ActionResult> RemovePlayerFromQueue(string lobbyId, string username)
        {
            if (_logicManager.TryGetValue(lobbyId, out var logic))
            {
                var userQueue = logic.GetQueue();
                if (userQueue != null && userQueue.Contains(username))
                {
                    // Remove the player from the queue
                    userQueue = new Queue<string>(userQueue.Where(user => user != username));
                    logic.SetQueue(userQueue);
                    return new ActionResult(true, null);
                }
                else
                {
                    return new ActionResult(false, "Player not found in the queue.");
                }
            }
            else
            {
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        public async Task<ActionResult> AddPlayerToQueue(string lobbyId, string username)
        {
            if (_logicManager.TryGetValue(lobbyId, out var logic))
            {
                var userQueue = logic.GetQueue();
                if (userQueue != null && !userQueue.Contains(username))
                {
                    // Add the player to the queue
                    userQueue.Enqueue(username);
                    logic.SetQueue(userQueue);
                    return new ActionResult(true, null);
                }
                else
                {
                    return new ActionResult(false, "Player is already in the queue.");
                }
            }
            else
            {
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        //public async Task<ActionResult<List<char>>> GetGuessedChars(string lobbyId)
        //{
        //    if (_logicManager.TryGetValue(lobbyId, out var logic))
        //    {
        //        var guessedLetters = logic.GetGuessedLetters();
        //        return new(true, null, guessedLetters);
        //    }
        //    else
        //    {
        //        return new(false, "Lobby does not exist", []);
        //    }

        //}
    }
}
