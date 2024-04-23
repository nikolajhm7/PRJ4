using Server.API.DTO;
using Server.API.Models;
using Server.API.Services.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Server.API.Services
{
    public class LobbyManager : ILobbyManager
    {
        private readonly IIdGenerator _idGenerator;
        public readonly Dictionary<string, Lobby> lobbies = [];

        public LobbyManager(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
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

        public string CreateNewLobby(ConnectedUserDTO user, int gameId)
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

            var lobby = new Lobby(lobbyId, user.ConnectionId, gameId);
            lobby.Members.Add(user);
            lobbies.Add(lobbyId, lobby);

            return lobbyId;
        }

        public void AddToLobby(ConnectedUserDTO user, string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
                lobby.Members.Add(user);
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
                return GameStatus.InGame;
            }
            return GameStatus.NO_LOBBY;
        }
    }
}
