using Microsoft.AspNetCore.SignalR;
using Server.API.Models;
using Server.API.DTO;
using Server.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Server.API.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        public record ActionResult(bool Success, string? Msg);

        private static readonly Dictionary<string, Lobby> lobbies = [];
        private readonly ILogger<LobbyHub> _logger;

        public LobbyHub(ILogger<LobbyHub> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> CreateLobby()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogDebug("Start generation of random lobby ID.");
            var lobbyId = IDGenerator.GenerateRandomLobbyID();
            _logger.LogDebug("Random lobby ID {LobbyId} generated.", lobbyId);

            var lobby = new Lobby(lobbyId, Context.ConnectionId);
            lobby.Members.Add(new ConnectedUserDTO(username, Context.ConnectionId));
            lobbies.Add(lobbyId, lobby);

            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            _logger.LogInformation("Lobby {LobbyId} created by {UserName}.", lobbyId, username);

            return new ActionResult(true, lobbyId);
        }

        [Authorize(Policy = "Guest+")]
        public async Task<ActionResult> JoinLobby(string lobbyId)
        {
            _logger.LogDebug("Attempting to join lobby {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                var user = new ConnectedUserDTO(username, Context.ConnectionId);

                lobby.Members.Add(user);
                await Clients.Group(lobbyId).SendAsync("UserJoinedLobby", user);
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

                _logger.LogInformation("{UserName} joined lobby {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new ActionResult(true, null);
            }
            else
            {
                _logger.LogError("Attemp to join non-existing lobby {LobbyId}.", lobbyId);
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        [Authorize(Policy = "Guest+")]
        public async Task<ActionResult> LeaveLobby(string lobbyId)
        {
            _logger.LogDebug("Attempting to leave lobby {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);
            
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                var user = new ConnectedUserDTO(username, Context.ConnectionId);

                lobby.Members.Remove(user);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                await Clients.Group(lobbyId).SendAsync("UserLeftLobby", user);

                _logger.LogInformation("{UserName} left lobby {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new ActionResult(true, null);
            }
            else
            {
                _logger.LogError("Attemp to leave non-existing lobby {LobbyId}.", lobbyId);
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        public async Task<ActionResult> StartGame(string lobbyId)
        {
            _logger.LogDebug("Attempting to start game in lobby {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby) && lobby.HostConnectionId == Context.ConnectionId)
            {
                // TODO: ADD START GAME LOGIC HERE

                await Clients.Group(lobbyId).SendAsync("GameStarted");

                // TODO: ADD GAME LOGIC CONTINUATION HERE

                _logger.LogInformation("{UserName} started game in lobby {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new ActionResult(true, null);
            }

            _logger.LogError("Attempt to start game on non-existing lobby {LobbyId}.", lobbyId);
            return new ActionResult(false, "Lobby does not exist.");
        }

        [Authorize(Policy = "Guest+")]
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Handling disconnect of user {UserName}.", Context.User?.Identity?.Name);
            if (exception != null)
            {
                _logger.LogError(exception, "Error on disconnect by {UserName}.", Context.User?.Identity?.Name);
            }
            else
            {
                _logger.LogInformation("{UserName} disconnected.", Context.User?.Identity?.Name);
            }

            var username = Context.User?.Identity?.Name;
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobby = lobbies.FirstOrDefault(x => x.Value.Members.Contains(user)).Value;
            if (lobby != null)
            {
                if (lobby.HostConnectionId == Context.ConnectionId)
                {
                    foreach(var member in lobby.Members)
                    {
                        await Clients.Client(member.ConnectionId).SendAsync("LobbyClosed");

                        await Groups.RemoveFromGroupAsync(member.ConnectionId, lobby.LobbyId);
                        lobbies.Remove(lobby.LobbyId);
                    }
                }
                await LeaveLobby(lobby.LobbyId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}