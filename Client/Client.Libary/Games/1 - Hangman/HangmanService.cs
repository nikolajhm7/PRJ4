using Client.Libary.Models;
using Client.Libary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Library.Games
{
    public class HangmanService : ConnectionService, IHangmanService
    {
        public HangmanService(string url) : base(url)
        {
            On<int>("GameStarted", (wordLength) =>
                           GameStartedEvent?.Invoke(wordLength));

            On<char, bool, List<int>>("GuessResult", (letter, isCorrect, positions) =>
                           GuessResultEvent?.Invoke(letter, isCorrect, positions));

            On<bool>("GameOver", (win) =>
                           GameOverEvent?.Invoke(win));

            On("LobbyClosed", () =>
                           LobbyClosedEvent?.Invoke());

            On<string>("UserLeftLobby", (username) =>
                           UserLeftLobbyEvent?.Invoke(username));
        }

        public event Action<int>? GameStartedEvent;
        public event Action<char, bool, List<int>>? GuessResultEvent;
        public event Action<bool>? GameOverEvent;
        public event Action? LobbyClosedEvent;
        public event Action<string>? UserLeftLobbyEvent;
        public async Task<ActionResult> StartGame(string lobbyId)
        {
            return await InvokeAsync("StartGame", lobbyId);
        }
        public async Task<ActionResult> GuessLetter(string lobbyId, char letter)
        {
            return await InvokeAsync("GuessLetter", lobbyId, letter);
        }
        public async Task<ActionResult> RestartGame(string lobbyId)
        {
            return await InvokeAsync("RestartGame", lobbyId);
        }
    }
}
