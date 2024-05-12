using Server.API.DTO;
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

        public bool IsHost(string username, string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return lobby.HostUsername == username;
            }
            return false;
        }

        public string? GetLobbyIdFromUsername(string username)
        {
            var lobby = lobbies.FirstOrDefault(x => x.Value.Members.Any(member => member.Username == username)).Value;
            if (lobby != null)
            {
                return lobby.LobbyId;
            }
            else return null;
        }

        public void UpdateUserInLobby(ConnectedUserDTO newUser, string lobbyId)
        {
            if(lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                var oldUser = lobby.Members.FirstOrDefault(x => x.Username == newUser.Username);
                if (oldUser != null)
                {
                    lobby.Members.Remove(oldUser);
                    lobby.Members.Add(newUser);
                }
            }
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
            var lobby = new Lobby(lobbyId, user.Username, gameId, maxPlayers);
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
        public ActionResult<int> GetLobbyGameId(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return new(true, "Succesfully returned lobby", lobby.GameId);
            }
            else
            {
                return new(false, "Could not find lobby", 0);
            }
        }

        public int GetLobbyMaxPlayers(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                return lobby.MaxPlayers;
            }
            else
            {
                return 0;
            }
        }
    }
}
