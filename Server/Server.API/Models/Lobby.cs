using Server.API.DTO;

namespace Server.API.Models
{
    public enum GameStatus
    {
        NO_LOBBY,
        InLobby,
        InGame
    }
    public class Lobby(string lobbyId, string hostConnectionId)
    public class Lobby(string lobbyId, string hostConnectionId, int gameId)
    {
        public string LobbyId { get; set; } = lobbyId;
        public HashSet<ConnectedUserDTO> Members { get; set; } = new HashSet<ConnectedUserDTO>();
        public string HostConnectionId { get; } = hostConnectionId;
        public GameStatus Status { get; set; } = GameStatus.InLobby;
        public int GameId { get; set; } = gameId;
    }
}
