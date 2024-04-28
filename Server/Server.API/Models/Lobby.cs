using Server.API.DTO;

namespace Server.API.Models
{
    public enum GameStatus
    {
        NoLobby,
        InLobby,
        InGame
    }
    public class Lobby(string lobbyId, string hostUsername, int gameId, int maxPlayers)
    {
        public string LobbyId { get; set; } = lobbyId;
        public HashSet<ConnectedUserDTO> Members { get; set; } = new HashSet<ConnectedUserDTO>();
        public string HostUsername { get; } = hostUsername;
        public GameStatus Status { get; set; } = GameStatus.InLobby;
        public int GameId { get; set; } = gameId;
        public int MaxPlayers { get; set; } = maxPlayers;
    }
}
