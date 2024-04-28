using Client.Library.Services;
using Client.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Client.Library.Games;
using System.Windows.Input;
using Microsoft.Maui.Controls;
namespace Client.UI.ViewModels
{
    [QueryProperty(nameof(LobbyId), "LobbyId")]
    //[QueryProperty(nameof(Players), "Players")]
    public partial class GameViewModel : ObservableObject
    {
        private readonly IHangmanService _hangmanService;

        public GameViewModel(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;
            guessedChars = new ObservableCollection<char>();

            //_hangmanService.GameStartedEvent += OnGameStarted;
            //_hangmanService.GuessResultEvent += OnGuessResult;
            //_hangmanService.GameOverEvent += OnGameOver;

            //_hangmanService.GameStartedEvent += (wordLength) =>
            //{
            //    // Set the title
            //    Title = "Hangman";

            //    // Set the message
            //    Message = "Game started!";

            //    // Set the error counter
            //    ErrorCounter = 0;

            //    // Set the letters status
            //    WordToGuess = new ObservableCollection<string>(new string[wordLength]);

            //};

            //_hangmanService.GuessResultEvent += (letter, isCorrect, positions) =>
            //{
            //    // Update the letters status
            //    for (int i = 0; i < positions.Count; i++)
            //    {
            //        LettersStatus[positions[i]] = isCorrect ? letter.ToString() : "_";
            //    };

            //    // Update the error counter
            //    if (!isCorrect)
            //    {
            //        ErrorCounter++;
            //    }
            //};

            //_hangmanService.GameOverEvent += (didWin, word) =>
            //{
            //    // Set the message
            //    Message = didWin ? "You won!" : "You lost!";

            //    // Set the title
            //    Title = "Game Over";

            //    // Set the guess letter command
            //    GuessLetterCommand = new Command<char>((letter) => { });

            //    // Set the players
            //    Players = new ObservableCollection<string>();

            //    // Set the letters status
            //    WordToGuess = new ObservableCollection<string>(word.Select(c => c.ToString()));
            //};

            //_hangmanService.LobbyClosedEvent += () =>
            //{
            //    // Set the message
            //    Message = "Lobby closed!";

            //    // Set the title
            //    Title = "Game Over";

            //    // Set the submit letter command
            //    SubmitLetterCommand = new Command(() => { });

            //    // Set the guess letter command
            //    GuessLetterCommand = new Command<char>((letter) => { });

            //    // Set the players
            //    Players = new ObservableCollection<string>();

            //    // Set the letters status
            //    LettersStatus = new ObservableCollection<string>();
            //};

            //_hangmanService.UserLeftLobbyEvent += (username) =>
            //{
            //    // Remove the player
            //    Players.Remove(username);
            //};


        }

        // Define command properties
        public string Title { get; }
        public int Players { get; }
        [ObservableProperty] private int errorCounter;
        [ObservableProperty] private string? lobbyId;
        [ObservableProperty] private char? letter;
        [ObservableProperty] private ObservableCollection<string> wordToGuess;
        ObservableCollection<char> guessedChars;

        public ObservableCollection<char> GuessedChars
        {
            get { return guessedChars; }
            set { SetProperty(ref guessedChars, value); }
        }
        public void OnPageAppearing()
        {
            StartGame();
        }

        //private void OnGameStart()
        //{
        //    _hangmanService.
        //}

        [RelayCommand]
        private async Task StartGame()
        {
            try
            {
                await _hangmanService.StartGame(LobbyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
            }
        }
        [RelayCommand]
        private void GuessLetter(char letter)
        {
            guessedChars.Add(letter);
            //try
            //{
            //    await _hangmanService.GuessLetter(LobbyId, letter);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error guessing letter: {ex.Message}");
            //}
        }

        [RelayCommand]
        private async Task RestartGame()
        {
            try
            {
                await _hangmanService.RestartGame(LobbyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting game: {ex.Message}");
            }
        }

    }
}

