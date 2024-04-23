using Microsoft.AspNetCore.SignalR;
using Server.API.Models;
using Server.API.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Server.API.Services.Interfaces;

namespace Server.API.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        public record ActionResult(bool Success, string? Msg);

        public readonly Dictionary<string, Lobby> lobbies = [];

        private readonly ILogger<LobbyHub> _logger;
        private readonly IIdGenerator _idGen;

        public LobbyHub(ILogger<LobbyHub> logger, IIdGenerator idGen)
        {
            _logger = logger;
            _idGen = idGen;
        }

        public async Task<ActionResult> CreateLobby(int gameId)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogDebug("Start generation of random lobby ID.");
            var lobbyId = _idGen.GenerateRandomLobbyId();

            // TODO: ADD Auth, that ID doesnt already exist.

            _logger.LogDebug("Random lobby ID {LobbyId} generated.", lobbyId);

            var lobby = new Lobby(lobbyId, Context.ConnectionId, gameId);
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

                foreach (var member in lobby.Members)
                {
                    await Clients.Caller.SendAsync("UserJoinedLobby", member);
                }

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

            var username = Context.User?.Identity?.Name;
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobby = lobbies.FirstOrDefault(x => x.Value.Members.Contains(user)).Value;
            if (lobby != null)
            {
                // If host disconnects, close lobby and remove all members.
                if (lobby.HostConnectionId == Context.ConnectionId)
                {
                    await Clients.Group(lobby.LobbyId).SendAsync("LobbyClosed");

                    foreach(var member in lobby.Members)
                    {
                        await Groups.RemoveFromGroupAsync(member.ConnectionId, lobby.LobbyId);
                    }

                    lobbies.Remove(lobby.LobbyId);
                }
                else
                {
                    await Clients.Group(lobby.LobbyId).SendAsync("UserLeftLobby", user);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobby.LobbyId);
                    lobby.Members.Remove(user);
                    _logger.LogInformation("{UserName} successfully left lobby {LobbyId}.", username, lobby.LobbyId);
                }
            }
            _logger.LogError(exception, "Successfully disconnected {UserName}.", username);
            await base.OnDisconnectedAsync(exception);
        }
    }
}