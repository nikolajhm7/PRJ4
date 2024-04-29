using Server.API.DTO;

namespace Server.API.Models
{
    public enum GameStatus
    {
        NoLobby,
        InLobby,
        InGame
    }
    public class Lobby(string lobbyId, string hostConnectionId, int gameId, int maxPlayers)
    {
        public string LobbyId { get; set; } = lobbyId;
        public HashSet<ConnectedUserDTO> Members { get; set; } = new HashSet<ConnectedUserDTO>();
        public string HostConnectionId { get; } = hostConnectionId;
        public GameStatus Status { get; set; } = GameStatus.InLobby;
        public int GameId { get; set; } = gameId;
        public int MaxPlayers { get; set; } = maxPlayers;
    }
}
