﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Client.Library.Services;
using Client.Library.DTO;
using Client.Library.Models;

namespace Client.Library.Services
{
    public class LobbyService : ConnectionService, ILobbyService
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

        public async Task<ActionResult> CreateLobbyAsync(int gameId)
        {
                return await InvokeAsync("CreateLobby", gameId);
        }

        public async Task<ActionResult> JoinLobbyAsync(string lobbyId)
        {
            return await InvokeAsync("JoinLobby", lobbyId);

        }

        public async Task<ActionResult<int>> GetLobbyGameId(string lobbyId)
        {
            return await InvokeAsync<int>("GetLobbyGameId", lobbyId);
        }

        public async Task<ActionResult<int>> GetLobbyMaxPlayers(string lobbyId)
        {
            return await InvokeAsync<int>("GetLobbyMaxPlayers", lobbyId);
        }

        public async Task<ActionResult> UserIsHost(string lobbyId)
        {
            return await InvokeAsync("UserIsHost", lobbyId);
        }

        public async Task<ActionResult> LeaveLobbyAsync(string lobbyId)
        {
            return await InvokeAsync("LeaveLobby", lobbyId);
        }

        public async Task<ActionResult<List<ConnectedUserDTO>>> GetUsersInLobby(string lobbyId)
        {
            return await InvokeAsync<List<ConnectedUserDTO>>("GetUsersInLobby", lobbyId);
        }

        public async Task<ActionResult> StartGameAsync(string lobbyId)
        {
            return await InvokeAsync("StartGame", lobbyId);
        }
    }
}
