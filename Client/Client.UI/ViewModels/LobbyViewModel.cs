using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Library.Models;
using System.Diagnostics;
using Client.Library.DTO;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Client.UI.Views;
using Client.Library.Constants;
using Client.UI.Games;
using Client.UI.ViewModels.Manager;
using Client.Library.Games;
using Microsoft.Extensions.Logging;


namespace Client.UI.ViewModels
{

    [QueryProperty(nameof(LobbyId), "LobbyId")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private readonly IHangmanService _hangmanService;
        private GameInfo _gameInfo;
        private int gameId;
        private bool isHost, gameStarted, _initialized = false;
        private ViewModelFactory _viewModelFactory;
        
        private ILogger<LobbyViewModel> _logger;

        [ObservableProperty] 
        private string imagePath = "";

        // Observable properties
        [ObservableProperty] 
        private string lobbyId = "000000";

        [ObservableProperty]
        public ObservableCollection<string> playerNames = new ObservableCollection<string> { };

        [ObservableProperty]
        private bool isGoToGameButtonEnabled = false;

        [ObservableProperty]
        private string goToGameButtonText = "Start game";

        public LobbyViewModel(ILobbyService lobbyService, INavigationService navigationService, ViewModelFactory viewModelFactory, IHangmanService hangmanService, ILogger<LobbyViewModel> logger)
        {
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            _hangmanService = hangmanService;
            _logger = logger;

            // Subscribe to events
            _lobbyService.UserJoinedLobbyEvent += OnUserJoinedLobby;
            _lobbyService.UserLeftLobbyEvent += OnUserLeftLobby;
            _lobbyService.GameStartedEvent += OnGameStarted;
            _lobbyService.LobbyClosedEvent += OnLobbyClosed;
        }

        public async Task OnPageappearing()
        {
            if(!_initialized)
            {
                await InitializeLobby();
                var isHostResult = await _lobbyService.UserIsHost(lobbyId);
                if (isHostResult.Success)
                {
                    //The player is the host of the game
                    isHost = true;
                    IsGoToGameButtonEnabled = true;
                }
                else
                {
                    //The player is a participant in the game
                    GoToGameButtonText = "Go to game";
                }
                await LoadUsersInLobby();
                _initialized = true;
            }
        }

        private async Task InitializeLobby()
        {
            _logger.LogInformation("Initializing lobby");
            var gameIdResult = await _lobbyService.GetLobbyGameId(lobbyId);
            if (!gameIdResult.Success)
            {
                _logger.LogError("Failed to get game id for lobby: " + gameIdResult.Msg);
                return;
            }
            gameId = gameIdResult.Value;
            if (GameInfMapper.GameInfoDictionary.TryGetValue(gameId, out GameInfo? _gameInfo))
            {
                ImagePath = _gameInfo.ImagePath;
            }
            else
            {
                _logger.LogError("Failed to get game info for game id: " + gameId);
            }
        }

        private async Task LoadUsersInLobby()
        {
            _logger.LogInformation("Loading users in lobby");
            var result = await _lobbyService.GetUsersInLobby(lobbyId);
            if (result.Success)
            {
                foreach (var user in result.Value)
                {
                    PlayerNames.Add(user.Username);
                }
                _logger.LogInformation("Successfully loaded users in lobby");
            }
            else
            {
                _logger.LogError("Failed to get users in lobby: " + result.Msg);
            }
        }

        private void OnUserJoinedLobby(ConnectedUserDTO user)
        {
            PlayerNames.Add(user.Username);
        }

        private void OnUserLeftLobby(ConnectedUserDTO user)
        {
            PlayerNames.Remove(user.Username);
        }

        private void OnGameStarted()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!isHost)
                {
                    GoToGameAsync();
                    IsGoToGameButtonEnabled = true;
                }
                else
                {
                    GoToGameButtonText = "Go back to game";
                }
            });
        }

        private async void GoToGameAsync()
        {
            await _navigationService.NavigateToPage($"{nameof(HangmanPage)}?LobbyId={LobbyId}");
        }

        private void OnLobbyClosed()
        {
            if(!isHost)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    //remove event listeners
                    _lobbyService.LobbyClosedEvent -= OnLobbyClosed;
                    CloseLobby();
                    LeaveLobbyAndServices();
                });
            }
        }

        private async void CloseLobby()
        {
            Debug.WriteLine("CloseLobby called");
            await Shell.Current.DisplayAlert(
                "Lobby closed",
                "Host closed lobby",
                "Ok"
            );
            _viewModelFactory.ResetHangmanViewModel();
        }


        [RelayCommand]
        async Task GoBack()
        {

            bool answer;
            if (isHost)
            {
                answer = await Shell.Current.DisplayAlert(
                    "Closing lobby",
                    "Going back will close the lobby and players will be kicked, proceed?",
                    "Yes",
                    "Cancel"
                );
            }
            else {
                answer = await Shell.Current.DisplayAlert(
                    "Leaving lobby",
                    "Going back will remove you from the lobby, proceed?",
                    "Yes",
                    "Cancel"
                );
            }

            if (answer)
            {
                LeaveLobbyAndServices();
            }
        }

        [RelayCommand]
        async Task GoToGame()
        {
            if (!gameStarted)
            {
                var res = await _lobbyService.StartGameAsync(LobbyId);
                if (!res.Success)
                {
                    await Shell.Current.DisplayAlert("Failed", "to join lobby", "OK");
                }
                gameStarted = true;
            }
            await _navigationService.NavigateToPage($"{nameof(HangmanPage)}?LobbyId={LobbyId}");
        }


        private async void LeaveLobbyAndServices()
        {
            _viewModelFactory.ResetHangmanViewModel();
            await _lobbyService.LeaveLobbyAsync(lobbyId);
            await _navigationService.NavigateToPage(nameof(PlatformPage));
            await _lobbyService.DisconnectAsync();
            await _hangmanService.DisconnectAsync();
        }
    }
}
