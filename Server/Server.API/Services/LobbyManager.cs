﻿using Server.API.DTO;
using Server.API.Models;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Server.API.Services
{
    public class LobbyManager : ILobbyManager
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IServiceProvider _serviceProvider;
        public readonly Dictionary<string, Lobby> lobbies = [];

        public LobbyManager(IIdGenerator idGenerator, IServiceProvider serviceProvider)
        {
            _idGenerator = idGenerator;
            _serviceProvider = serviceProvider;
        }

        public bool LobbyExists(string lobbyId)
        {
            return lobbies.ContainsKey(lobbyId);
        }

        public bool IsHost(string connectionId, string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return lobby.HostConnectionId == connectionId;
            }
            return false;
        }

        public string? GetLobbyIdFromUser(ConnectedUserDTO user)
        {
            var lobby = lobbies.FirstOrDefault(x => x.Value.Members.Contains(user)).Value;
            if (lobby != null)
            {
                return lobby.LobbyId;
            }
            else return null;
        }

        public List<ConnectedUserDTO> GetUsersInLobby(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return lobby.Members.ToList();
            }
            else return [];
        }

        public async Task<string> CreateNewLobby(ConnectedUserDTO user, int gameId)
        {
            string? lobbyId = null;
            while (lobbyId == null)
            { 
                lobbyId = _idGenerator.GenerateRandomLobbyId();
                if (lobbies.ContainsKey(lobbyId))
                {
                    lobbyId = null;
                }
            }

            var gameRepository = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IGameRepository>();

            var maxPlayers = await gameRepository.GetMaxPlayers(gameId);
            var lobby = new Lobby(lobbyId, user.ConnectionId, gameId, maxPlayers);
            lobby.Members.Add(user);
            lobbies.Add(lobbyId, lobby);

            return lobbyId;
        }

        public ActionResult<List<ConnectedUserDTO>> AddToLobby(ConnectedUserDTO user, string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                if (lobby.Members.Count >= lobby.MaxPlayers) return new(false, "Lobby is full", []);

                var users = lobby.Members.ToList();
                lobby.Members.Add(user);
                return new(true, null, users);
            }
            else return new(false, "Could not find lobby", []);
        }

        public void RemoveFromLobby(ConnectedUserDTO user, string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
                lobby.Members.Remove(user);
        }

        public void RemoveLobby(string lobbyId)
        {
            lobbies.Remove(lobbyId);
        }

        public void StartGame(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                lobby.Status = GameStatus.InGame;
            }
        }

        public GameStatus GetGameStatus(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return lobby.Status;
            }
            return GameStatus.NoLobby;
        }
        public ActionResult<Lobby> GetLobbyInfo(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return new(true, "Succesfully returned lobby", lobby);
            }
            else
            {
                return new(false, "Could not find lobby", null);
            }
        }
    }
}
