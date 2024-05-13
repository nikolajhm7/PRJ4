using Client.Library.DTO;
using Client.Library.Models;
using Client.Library.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Library.Games
{
    public class HangmanService : ConnectionService, IHangmanService
    {
        public HangmanService(IConfiguration configuration) : base(configuration["ConnectionSettings:ApiUrl"] + configuration["ConnectionSettings:HangmanEndpoint"])
        {
            On<int>("GameStarted", (wordLength) =>
                           GameStartedEvent?.Invoke(wordLength));

            On<char, bool, List<int>>("GuessResult", (letter, isCorrect, positions) =>
                           GuessResultEvent?.Invoke(letter, isCorrect, positions));

            On<bool, string>("GameOver", (didWin, word) =>
                           GameOverEvent?.Invoke(didWin, word));

            On("LobbyClosed", () =>
                           LobbyClosedEvent?.Invoke());

            On<string>("UserLeftLobby", (username) =>
                           UserLeftLobbyEvent?.Invoke(username));
        }

        public event Action<int>? GameStartedEvent;
        public event Action<char, bool, List<int>>? GuessResultEvent;
        public event Action<bool, string>? GameOverEvent;
        public event Action? LobbyClosedEvent;
        public event Action<string>? UserLeftLobbyEvent;
        public async Task<ActionResult> GuessLetter(string lobbyId, char letter)
        {
            return await InvokeAsync("GuessLetter", lobbyId, letter);
        }
        public async Task<ActionResult> RestartGame(string lobbyId)
        {
            return await InvokeAsync("RestartGame", lobbyId);
        }
        public async Task<ActionResult<List<ConnectedUserDTO>>> GetUsersInGame(string lobbyId)
        {
            return await InvokeAsync<List<ConnectedUserDTO>>("GetUsersInGame", lobbyId);
        }

        public async Task<ActionResult<string>> GetFrontPlayerForGame(string lobbyId)
        {
            return await InvokeAsync<string>("GetFrontPlayerForGame", lobbyId);
        }

        public async Task InitQueueForGame(string lobbyId)
        {
            await InvokeAsync("InitQueueForGame", lobbyId);
        }

        public async Task<ActionResult> LeaveGameAsync(string lobbyId)
        {
            return await InvokeAsync("LeaveGame", lobbyId);
        }

        public async Task<ActionResult<List<char>>> GetGuessedLetters(string lobbyId)
        {
            return await InvokeAsync<List<char>>("GetGuessedChars", lobbyId);
        }
    }
}
