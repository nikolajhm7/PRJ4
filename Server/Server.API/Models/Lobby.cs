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
    {
        public string LobbyId { get; set; } = lobbyId;
        public HashSet<ConnectedUserDTO> Members { get; set; } = new HashSet<ConnectedUserDTO>();
        public string HostConnectionId { get; } = hostConnectionId;
        public int GameId { get; set; }
        public GameStatus Status { get; set; } = GameStatus.InLobby;
    }
}
