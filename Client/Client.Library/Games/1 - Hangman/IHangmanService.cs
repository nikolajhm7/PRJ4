using Client.Library.DTO;
using Client.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Library.Games
{
    public interface IHangmanService
    {
        event Action<int>? GameStartedEvent;
        event Action<char, bool, List<int>>? GuessResultEvent;
        event Action<bool, string>? GameOverEvent;
        event Action? LobbyClosedEvent;
        event Action<string>? UserLeftLobbyEvent;
        Task ConnectAsync();
        Task DisconnectAsync();
        //Task<ActionResult> StartGame(string lobbyId);
        Task<ActionResult> GuessLetter(string lobbyId, char letter);
        Task<ActionResult> RestartGame(string lobbyId);
    }
}
