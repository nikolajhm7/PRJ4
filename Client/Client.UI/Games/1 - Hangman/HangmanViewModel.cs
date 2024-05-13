using Client.Library.Services;
using Client.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Client.Library.Games;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading;
using Client.Library.Models;
using Client.Library.Services.Interfaces;

namespace Client.UI.Games
{
    [QueryProperty(nameof(LobbyId), "LobbyId")]
    //[QueryProperty(nameof(Players), "Players")]
    public partial class HangmanViewModel : ObservableObject
    {
        private readonly IHangmanService _hangmanService;
        private readonly INavigationService _navigationService;
        private readonly ILobbyService _lobbyService;
        private int ErrorCounter;
        private Queue<string> userQueue;
        private bool _initialized = false;
        private int maxPlayers = 0;

        // Define command properties
        [ObservableProperty]
        public ObservableCollection<string> playerNames = new ObservableCollection<string> { };

        [ObservableProperty] private string errorLabel;
        [ObservableProperty] private string lobbyIdLabel;
        [ObservableProperty] private string? lobbyId;
        [ObservableProperty] private string title = "Empty";
        [ObservableProperty] private string? statusMessage;
        [ObservableProperty] private string? playerStatus;
        [ObservableProperty] private string? players;
        [ObservableProperty] private char? letter;
        [ObservableProperty] private string hiddenWord;
        ObservableCollection<char> guessedChars;
        [ObservableProperty] private string? imageSource;
        [ObservableProperty] private string? frontPlayer;


        public ObservableCollection<char> GuessedChars
        {
            get { return guessedChars; }
            set
            {
                SetProperty(ref guessedChars, value);
            }
        }


        public HangmanViewModel(IHangmanService hangmanService, ILobbyService lobbyService, INavigationService navigationService)
        {
            _hangmanService = hangmanService;
            _navigationService = navigationService;
            _lobbyService = lobbyService;
            guessedChars = [];
            //playerNames.Add("Anthony");
            //playerNames.Add("Nikolaj");
            //playerNames.Add("user.Username");
            //playerNames.Add("user.Username");
            _hangmanService.GameStartedEvent += OnGameStarted;
            _hangmanService.GuessResultEvent += OnGuessResult;
            _hangmanService.GameOverEvent += OnGameOver;
            _hangmanService.LobbyClosedEvent += OnLobbyClosed;
            _hangmanService.UserLeftLobbyEvent += OnUserLeftLobby;

            //maxPlayers = _lobbyService.GetLobbyMaxPlayers(LobbyId).Result.Value;
        }
        public void unsubscribeServices()
        {
            _hangmanService.GameStartedEvent -= OnGameStarted;
            _hangmanService.GuessResultEvent -= OnGuessResult;
            _hangmanService.GameOverEvent -= OnGameOver;
            _hangmanService.LobbyClosedEvent -= OnLobbyClosed;
            _hangmanService.UserLeftLobbyEvent -= OnUserLeftLobby;
        }
        public async Task OnPageAppearing()
        {
            if (!_initialized)
            {
                await _hangmanService.ConnectAsync();
                GuessedChars.Clear();
                await LoadUsersInGame();
                _initialized = true;
            }
        }

        private async Task LoadUsersInGame()
        {
            await Task.Delay(1);
            var result = await _hangmanService.GetUsersInGame(LobbyId);
            if (result.Success)
            {
                foreach (var user in result.Value)
                {
                    PlayerNames.Add(user.Username);
                }
            }
            else
            {
                Debug.WriteLine("Failed to get users in lobby: " + result.Msg);
            }
        }

        private async Task LoadPlayerQueue()
        {
            await Task.Delay(1);
            var result = await _hangmanService.GetQueueForGame(LobbyId);
            if (result.Success)
            {
                userQueue = result.Value;
                FrontPlayer = userQueue.Peek() + "'s turn";
            }
        }

        //private void LoadPlayersTurn()
        //{
        //    var currentPlayer = userQueue.Dequeue();
        //    FrontPlayer = currentPlayer + "'s turn";
        //    userQueue.Enqueue(currentPlayer);
        //}

        private void MakeUnderscores(int wordLength)
        {
            for (int i = 0; i < wordLength; i++)
            {
                HiddenWord += "_";
            }
        }

        #region OnGameStarted
        private void OnGameStarted(int wordLength)
        {
            Debug.WriteLine($"Game started with wordLength: {wordLength}");

            // Set the title
            Title = "Welcome to Hangman!";

            // Set the message
            StatusMessage = $"Game started with wordLength: {wordLength}";
            PlayerStatus = $"Players: {PlayerNames.Count}/{maxPlayers}";

            // Set the lobby id
            LobbyIdLabel = $"Lobby ID: {LobbyId}";

            // Set the error counter
            ErrorCounter = 0;
            ErrorLabel = $"Errors: {ErrorCounter}";

            // List players

            // Set the image
            ImageSource = $"hangman_img{ErrorCounter}.jpg";

            HiddenWord = "";

            // Set the hidden word length
            MakeUnderscores(wordLength);

            GuessedChars.Clear();

            LoadPlayerQueue();
            //LoadPlayersTurn();

        }
        #endregion

        #region OnGuessResult
        private void OnGuessResult(char letter, bool isCorrect, List<int> positions)
        {
            Console.WriteLine($"Guess result: {letter}, {isCorrect}, {string.Join(",", positions)}");


            //Update the letters status
            if (isCorrect)
            {
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
                if (!GuessedChars.Contains(char.ToUpper(letter)))
                {
                    ErrorCounter++;
                    ErrorLabel = $"Errors: {ErrorCounter}";
                    ImageSource = $"hangman_img{ErrorCounter}.jpg";
                }
            }

            if (!guessedChars.Contains(char.ToUpper(letter))) { GuessedChars.Add(char.ToUpper(letter)); }

            LoadPlayerQueue();
            //LoadPlayersTurn();

        }
        #endregion

        #region OnGameOver
        private void OnGameOver(bool didWin, string word)
        {
            Console.WriteLine($"Game over: {didWin}, {word}");

            // Set the message
            StatusMessage = didWin ? $"You won!\nThe Hidden Word was: {word} " : $"You lost!\nThe Hidden Word was: {word}";

            // Set the title
            Title = "HangMan: Game Over";

        }
        #endregion


        private void OnLobbyClosed()
        {
            Console.WriteLine("Lobby closed!");

            // Set the message
            StatusMessage = "Lobby closed!";

            // Set the title
            Title = "HangMan: Game Over";

            // Close the lobby
            //await _navigationService.NavigateBack();

        }

        private void OnUserLeftLobby(string username)
        {
            Console.WriteLine($"User left lobby: {username}");



            //// Remove the player
            PlayerStatus = $"Players: {PlayerNames.Count}/{maxPlayers} - {username} has left";
            PlayerNames.Remove(username);
        }

        [RelayCommand]
        private async Task GuessLetter(char letter)
        {

            try
            {
                var response = await _hangmanService.GuessLetter(LobbyId, letter);
                //if(guessedChars.Contains(char.ToUpper(letter)) ) 
                //{ 
                //    await Shell.Current.DisplayAlert("Fejl", $"'{char.ToUpper(letter)}' er allerede gættet på!", "OK"); 
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guessing letter: {ex.Message}");
            }

        }

        [RelayCommand]
        async Task GoBack()
        {
            await _navigationService.NavigateBack();
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

//[RelayCommand]
//private async Task StartGame()
//{
//    try
//    {
//        await _hangmanService.StartGame(LobbyId);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error starting game: {ex.Message}");
//    }
//}

