﻿using Microsoft.AspNetCore.SignalR;
using Server.API.Services.Interfaces;
using Server.API.DTO;
using Server.API.Models;
using System.Linq.Expressions;

namespace Server.API.Games
{
    public class HangmanHub : Hub
    {
        private readonly ILogger<HangmanHub> _logger;
        private readonly ILobbyManager _lobbyManager;
        private readonly Dictionary<string, HangmanLogic> _lobbyLogic = [];

        public HangmanHub(ILobbyManager lobbyManager, ILogger<HangmanHub> logger)
        {
            _logger = logger;
            _lobbyManager = lobbyManager;
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
            var logic = new HangmanLogic(lobbyId);
            _lobbyLogic.Add(lobbyId, logic);

            var word = logic.StartGame();
            await Clients.Group(lobbyId).SendAsync("GameStarted", word.Length);
            return new(true, null);
        }

        public async Task<ActionResult> GuessLetter(string lobbyId, char letter)
        {
            if (_lobbyLogic.TryGetValue(lobbyId, out var logic))
            {
                var isCorrect = logic.GuessLetter(letter);
                await Clients.Group(logic.LobbyId).SendAsync("GuessResult", letter, isCorrect);

                var isWin = logic.IsGameOver();
                if (isWin)
                {
                    await Clients.Group(logic.LobbyId).SendAsync("GameOver", isWin);
                }

                return new(true, null);
            }
            return new(false, "Lobby does not exist.");
        }

        public async Task<ActionResult> RestartGame(string lobbyId)
        {
            if (_lobbyLogic.TryGetValue(lobbyId, out var logic))
            {
                var word = logic.RestartGame();
                await Clients.Group(logic.LobbyId).SendAsync("GameStarted", word.Length);
                return new(true, null);
            }
            return new(false, "Lobby does not exist.");
        }

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
                await Clients.Group(lobbyId).SendAsync("UserLeftLobby", user);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                _lobbyManager.RemoveFromLobby(user, lobbyId);
                _logger.LogInformation("{UserName} successfully left lobby {LobbyId}.", user.Username, lobbyId);
            }
        }
    }
}