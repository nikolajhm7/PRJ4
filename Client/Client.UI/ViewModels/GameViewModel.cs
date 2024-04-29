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
using System.Diagnostics;
namespace Client.UI.ViewModels
{
    [QueryProperty(nameof(LobbyId), "LobbyId")]
    //[QueryProperty(nameof(Players), "Players")]
    public partial class GameViewModel : ObservableObject
    {
        private readonly IHangmanService _hangmanService;
        // Define command properties
        [ObservableProperty] private int errorCounter;
        [ObservableProperty] private string? lobbyId;
        [ObservableProperty] private string? title;
        [ObservableProperty] private string? statusMessage;
        [ObservableProperty] private string? players;
        [ObservableProperty] private char? letter;
        [ObservableProperty] private string hiddenWord;
        ObservableCollection<char> guessedChars;
        [ObservableProperty] private string? imageSource;

        public ObservableCollection<char> GuessedChars
        {
            get { return guessedChars; }
            set 
            { 
                SetProperty(ref guessedChars, value); 
            }
        }

        //public ObservableCollection<string> HiddenWord
        //{
        //    get { return hiddenWord; }
        //    set
        //    {
        //        SetProperty(ref hiddenWord, value);
        //        OnPropertyChanged(nameof(HiddenWord));
        //    }
        //}


        public GameViewModel(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;
            guessedChars = new ObservableCollection<char>();
            //hiddenWord = new ObservableCollection<string>();

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
            Debug.WriteLine($"Game started with wordLength: {wordLength}");
            
            // Set the title
            Title = "Welcome to Hangman!";

            // Set the message
            StatusMessage = "Game started!";

            // Set the error counter
            ErrorCounter = 0;

            // List players

            // Set the image
            ImageSource = $"hangman_img{ErrorCounter}.jpg";

            // Set the hidden word length
            MakeUnderscores(wordLength);
            
        }

        private void MakeUnderscores(int wordLength)
        {
            for (int i = 0; i < wordLength; i++)
            {
                HiddenWord += "_";
            }
        }
        private void OnGuessResult(char letter, bool isCorrect, List<int> positions)
        {
            Console.WriteLine($"Guess result: {letter}, {isCorrect}, {string.Join(",", positions)}");


            //Update the letters status
            if (isCorrect) {
                //var hw = HiddenWord.ToCharArray();
                //for (int i = 0; i < positions.Count; i++)
                //{
                //    //HiddenWord[positions[i]] = isCorrect ? letter.ToString() : "_";
                //    hw[positions[i]] = letter;
                //};
                //HiddenWord = hw.ToString();

                var hwChars = HiddenWord.ToCharArray();
                foreach (var position in positions)
                {
                    hwChars[position] = letter;
                }
                HiddenWord = new string(hwChars).ToUpper();
            }
            // Update the error counter
            if (!isCorrect)
            {
                ErrorCounter++;
                ImageSource = $"hangman_img{ErrorCounter}.jpg";
            }
        }

        private void OnGameOver(bool didWin, string word)
        {
            Console.WriteLine($"Game over: {didWin}, {word}");

            // Set the message
            StatusMessage = didWin ? "You won!" : "You lost!";

            // Set the title
            Title = "Game Over";

            //// Set the letters status
            //WordLength = new ObservableCollection<string>();
        }

        private void OnLobbyClosed()
        {
            Console.WriteLine("Lobby closed!");

            // Set the message
            StatusMessage = "Lobby closed!";

            // Set the title
            Title = "Game Over";

            //// Set the letters status
            //WordLength = new ObservableCollection<string>();
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
                if (!guessedChars.Contains(letter)) { guessedChars.Add(letter); }
                else { await Shell.Current.DisplayAlert("Fejl", "Bogstav er allerede gættet på!", "OK"); }
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

