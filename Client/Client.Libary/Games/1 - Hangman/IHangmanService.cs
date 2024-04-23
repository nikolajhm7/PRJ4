﻿using Client.Libary.DTO;
using Client.Libary.Models;
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
        event Action<bool>? GameOverEvent;
        event Action? LobbyClosedEvent;
        event Action<string>? UserLeftLobbyEvent;
        Task<ActionResult> StartGame(string lobbyId);
        Task<ActionResult> GuessLetter(string lobbyId, char letter);
        Task<ActionResult> RestartGame(string lobbyId);
    }
}
