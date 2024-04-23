using Server.API.DTO;

namespace Server.API.Models
{
    public class Lobby(string lobbyId, string hostConnectionId, int gameId)
    {
        public string LobbyId { get; set; } = lobbyId;
        public HashSet<ConnectedUserDTO> Members { get; set; } = new HashSet<ConnectedUserDTO>();
        public string HostConnectionId { get; } = hostConnectionId;
        public int GameId { get; set; } = gameId;
    }
}
