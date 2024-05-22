using Client.Library.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Library.Games;
using System.Diagnostics;
using Client.Library.Services.Interfaces;

namespace Client.UI.Games
{
    [QueryProperty(nameof(LobbyId), "LobbyId")]
    //[QueryProperty(nameof(Players), "Players")]
    public partial class HangmanViewModel : ObservableObject
    {
        // Define private variables
        private readonly IHangmanService _hangmanService;
        private readonly INavigationService _navigationService;
        private readonly ILobbyService _lobbyService;
        private int ErrorCounter;
        private string _currentPlayer;
        private bool _initialized = false;
        private int _maxPlayers = 0;
        private int _playerCount = 0;
        private bool _queueInitialized = false;

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
        [ObservableProperty] private bool? gameIsDone = false;


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

            // Subscribe to events
            _hangmanService.GameStartedEvent += OnGameStarted;
            _hangmanService.GuessResultEvent += OnGuessResult;
            _hangmanService.GameOverEvent += OnGameOver;
            _hangmanService.LobbyClosedEvent += OnLobbyClosed;
            _hangmanService.UserLeftLobbyEvent += OnUserLeftLobby;
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

                // Set the player status variables
                var maxplayersResult = await _lobbyService.GetLobbyMaxPlayers(LobbyId);
                _maxPlayers = maxplayersResult.Value;
                _playerCount = PlayerNames.Count();
                PlayerStatus = $"Players: {_playerCount}/{_maxPlayers}";

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
            if (!_queueInitialized)
            {
                await _hangmanService.InitQueueForGame(LobbyId);
                _queueInitialized = true;
            }
            var result = await _hangmanService.GetFrontPlayerForGame(LobbyId);
            if (result.Success)
            {
                _currentPlayer = result.Value;
                FrontPlayer = _currentPlayer + "'s turn";
            }
        }

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

            //remove reset button
            GameIsDone = false;

            // Set the title
            Title = "Welcome to Hangman!";

            // Set the message
            StatusMessage = $"Game started with wordLength: {wordLength}";
            // Set the lobby id
            LobbyIdLabel = $"Lobby ID: {LobbyId}";
            PlayerStatus = $"Players: {_playerCount}/{_maxPlayers}";

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
        }
        #endregion

        #region OnGameOver
        private void OnGameOver(bool didWin, string word)
        {
            Console.WriteLine($"Game over: {didWin}, {word}");

            // Set the message
            StatusMessage = didWin ? "You won!" : $"You lost!\nThe hidden word was: {word.ToUpper()}";

            // Set the title
            Title = "Hangman: Game Over";

            // Make reset button visible
            GameIsDone = true;

            // Make reset button visible
            GameIsDone = true;

        }
        #endregion


        private void OnLobbyClosed()
        {
            Console.WriteLine("Lobby closed!");

            // Set the message
            StatusMessage = "Lobby closed!";

            // Set the title
            Title = "Hangman: Game Over";

            // Close the lobby
            //await _navigationService.NavigateBack();

        }

        private void OnUserLeftLobby(string username)
        {
            Console.WriteLine($"User left lobby: {username}");

            // Remove player
            PlayerNames.Remove(username);

            // Update max players
            _playerCount = PlayerNames.Count();

            // Update player status
            PlayerStatus = $"Players: {_playerCount}/{_maxPlayers} - {username} has left";
        }

        [RelayCommand]
        private async Task GuessLetter(char letter)
        {

            try
            {
                var response = await _hangmanService.GuessLetter(LobbyId, letter);
                //if(guessedChars.Contains(char.ToUpper(letter)) ) 
                //{ 
                //    await Shell.Current.DisplayAlert("Fejl", $"'{char.ToUpper(letter)}' er allerede g�ttet p�!", "OK"); 
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

