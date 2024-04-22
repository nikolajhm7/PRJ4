using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Libary.Models;
using Client.Libary.DTO;

namespace Client.Libary.Services
{
    public interface ILobbyService
    {
        public event Action<ConnectedUserDTO>? UserJoinedLobbyEvent;
        public event Action<ConnectedUserDTO>? UserLeftLobbyEvent;
        public event Action? GameStartedEvent;
        public event Action? LobbyClosedEvent;

        Task<ActionResult> CreateLobbyAsync();

        Task<ActionResult> JoinLobbyAsync(string lobbyId);

        Task<ActionResult> LeaveLobbyAsync(string lobbyId);

        Task<ActionResult> StartGameAsync(string lobbyId);
    }
}
