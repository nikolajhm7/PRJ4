using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Library.DTO;
using Client.Library.Models;

namespace Client.Library.Services
{
    public interface ILobbyService
    {
        public event Action<ConnectedUserDTO>? UserJoinedLobbyEvent;
        public event Action<ConnectedUserDTO>? UserLeftLobbyEvent;
        public event Action? GameStartedEvent;
        public event Action? LobbyClosedEvent;

        Task<ActionResult> CreateLobbyAsync(int gameId);

        Task<ActionResult<List<ConnectedUserDTO>>> JoinLobbyAsync(string lobbyId);

        Task<ActionResult> LeaveLobbyAsync(string lobbyId);

        Task<ActionResult> StartGameAsync(string lobbyId);
    }
}
