using Client.Library.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Library.Models
{
    public enum GameStatus
    {
        NO_LOBBY,
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
