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
        // Define command properties
        public string Title { get; set; }
        public string StatusMessage { get; set; }
        public int Players { get; }
        [ObservableProperty] private int errorCounter;
        [ObservableProperty] private string? lobbyId;
        [ObservableProperty] private char? letter;
        [ObservableProperty] private ObservableCollection<string> wordLength;
        ObservableCollection<char> guessedChars;
        [ObservableProperty] private string imageSource = "HangmanImages/img0.jpg";
        public ObservableCollection<char> GuessedChars
        {
            get { return guessedChars; }
            set { SetProperty(ref guessedChars, value); }
        }

        public GameViewModel(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;
            guessedChars = new ObservableCollection<char>();

            _hangmanService.GameStartedEvent += OnGameStarted;
            _hangmanService.GuessResultEvent += OnGuessResult;
            _hangmanService.GameOverEvent += OnGameOver;
            _hangmanService.LobbyClosedEvent += OnLobbyClosed;
            _hangmanService.UserLeftLobbyEvent += OnUserLeftLobby;
        }
        public void OnPageAppearing()
        {
            StartGame();
        }

        private void OnGameStarted(int wordLength)
        {
            Console.WriteLine($"Game started with wordlength: {wordLength}");
            
            // Set the title
            Title = "Welcome to Hangman!";

            // Set the message
            StatusMessage = "Game started!";

            // Set the error counter
            ErrorCounter = 0;

            // Set the letters status
            WordLength = new ObservableCollection<string>(new string[wordLength]);
        }

        private void OnGuessResult(char letter, bool isCorrect, List<int> positions)
        {
            Console.WriteLine($"Guess result: {letter}, {isCorrect}, {string.Join(",", positions)}");

            // Update the letters status
            for (int i = 0; i < positions.Count; i++)
            {
                WordLength[positions[i]] = isCorrect ? letter.ToString() : "_";
            };

            // Update the error counter
            if (!isCorrect)
            {
                ErrorCounter++;
                ImageSource = $"HangmanImages/img{ErrorCounter}.jpg";
            }
        }

        private void OnGameOver(bool didWin, string word)
        {
            Console.WriteLine($"Game over: {didWin}, {word}");

            // Set the message
            StatusMessage = didWin ? "You won!" : "You lost!";

            // Set the title
            Title = "Game Over";

            // Set the letters status
            WordLength = new ObservableCollection<string>();
        }

        private void OnLobbyClosed()
        {
            Console.WriteLine("Lobby closed!");

            // Set the message
            StatusMessage = "Lobby closed!";

            // Set the title
            Title = "Game Over";

            // Set the letters status
            WordLength = new ObservableCollection<string>();
        }

        private void OnUserLeftLobby(string username)
        {
            Console.WriteLine($"User left lobby: {username}");

            //// Remove the player
            //Players.Remove(username);
        }

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
        private async Task GuessLetter(char letter)
        {
            try
            {
                var response = await _hangmanService.GuessLetter(LobbyId, letter);

                //if (response.Msg != "No connection to server.")
                //{
                //    guessedChars.Add(letter);
                //}
                guessedChars.Add(letter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guessing letter: {ex.Message}");
            }
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

