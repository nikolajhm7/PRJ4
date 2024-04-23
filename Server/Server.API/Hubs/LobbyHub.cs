﻿using Microsoft.AspNetCore.SignalR;
using Server.API.Models;
using Server.API.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Server.API.Services.Interfaces;
using Server.API.Services;
using System.Text.RegularExpressions;
using Server.API.Repositories;

namespace Server.API.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        //public readonly Dictionary<string, Lobby> lobbies = [];

        private readonly ILogger<LobbyHub> _logger;
        private readonly IIdGenerator _idGen;
        private readonly ILobbyManager _lobbyManager;

        public LobbyHub(ILogger<LobbyHub> logger, IIdGenerator idGen)
        {
            _logger = logger;
            _idGen = idGen;
            _lobbyManager = new LobbyManager(_idGen);
        }

        public LobbyHub(ILogger<LobbyHub> logger, IIdGenerator idGen, ILobbyManager lobbyManager)
        {
            _logger = logger;
            _idGen = idGen;
            _lobbyManager = lobbyManager;
        }

        public async Task<ActionResult> CreateLobby(int gameId)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            var lobbyId = _lobbyManager.CreateNewLobby(new ConnectedUserDTO(username, Context.ConnectionId), gameId);

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

            if (_lobbyManager.LobbyExists(lobbyId))
            {
                var user = new ConnectedUserDTO(username, Context.ConnectionId);

                foreach (var member in _lobbyManager.GetUsersInLobby(lobbyId))
                {
                    await Clients.Caller.SendAsync("UserJoinedLobby", member);
                }

                _lobbyManager.AddToLobby(user, lobbyId);

                await Clients.Group(lobbyId).SendAsync("UserJoinedLobby", user);
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

                _logger.LogInformation("{UserName} joined lobby {LobbyId}.", Context.User?.Identity?.Name, lobbyId);
                return new ActionResult(true, null);
            }
            else
            {
                _logger.LogError("Attempt to join non-existing lobby {LobbyId}.", lobbyId);
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

        public async Task<ActionResult> StartGame(string lobbyId)
        {
            _logger.LogDebug("Attempting to start game in lobby {LobbyId} by user {UserName}.", lobbyId, Context.User?.Identity?.Name);

            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            if (_lobbyManager.IsHost(Context.ConnectionId, lobbyId))
            {
                _lobbyManager.StartGame(lobbyId);
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

            var lobbyId = _lobbyManager.GetLobbyIdFromUser(user);
            if (lobbyId != null)
            {
                // If game is started we expect users to disconnect
                if (_lobbyManager.GetGameStatus(lobbyId) == GameStatus.InGame)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                    await base.OnDisconnectedAsync(exception);
                    return;
                }

                await RemoveUserFromLobby(lobbyId, user);
            }
            _logger.LogDebug("Successfully disconnected {UserName}.", username);
            await base.OnDisconnectedAsync(exception);
        }

        // Funktion til simplificering af at fjerne user
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
                await Clients.Group(lobbyId).SendAsync("UserLeftLobby", user);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                _lobbyManager.RemoveFromLobby(user, lobbyId);
                _logger.LogInformation("{UserName} successfully left lobby {LobbyId}.", user.Username, lobbyId);
            }
        }
    }
}