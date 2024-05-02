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


namespace Client.UI.ViewModels
{

    [QueryProperty(nameof(LobbyId), "LobbyId")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private GameInfo _gameInfo;
        private int gameId;
        private bool isHost = false;

        [ObservableProperty] 
        private string imagePath = "";

        // Observable properties
        [ObservableProperty] 
        private string lobbyId = "000000";

        [ObservableProperty]
        public ObservableCollection<string> playerNames = new ObservableCollection<string> { };

        [ObservableProperty]
        private bool isGoToGameButtonVisible = false;

        public LobbyViewModel(ILobbyService lobbyService, INavigationService navigationService)
        {
            _lobbyService = lobbyService;
            _navigationService = navigationService;

            // Subscribe to events
            _lobbyService.UserJoinedLobbyEvent += OnUserJoinedLobby;
            _lobbyService.UserLeftLobbyEvent += OnUserLeftLobby;
            _lobbyService.GameStartedEvent += OnGameStarted;
            _lobbyService.LobbyClosedEvent += OnLobbyClosed;
        }

        public async Task OnPageappearing()
        {
            await InitializeLobby();
            var isHostResult = await _lobbyService.UserIsHost(lobbyId);
            if (isHostResult.Success)
            {
                isHost = true;
                IsGoToGameButtonVisible = true;
            }
            await LoadUsersInLobby();
        }

        private async Task InitializeLobby()
        {
            var gameIdResult = await _lobbyService.GetLobbyGameId(lobbyId);
            if (!gameIdResult.Success)
            {
                Debug.WriteLine("Failed to get gameid in lobby: " + gameIdResult.Msg);
            }
            gameId = gameIdResult.Value;
            if (GameInfMapper.GameInfoDictionary.TryGetValue(gameId, out GameInfo? _gameInfo))
            {
                ImagePath = _gameInfo.ImagePath;
            }
            else
            {
                Debug.WriteLine("Lobby info not defined for gameid " + gameId);
            }
        }

        private async Task LoadUsersInLobby()
        {
            var result = await _lobbyService.GetUsersInLobby(lobbyId);
            if (result.Success)
            {
                foreach (var user in result.Value)
                {
                    playerNames.Add(user.Username);
                }
            }
            else
            {
                Debug.WriteLine("Failed to get users in lobby: " + result.Msg);
            }
        }

        private void OnUserJoinedLobby(ConnectedUserDTO user)
        {
            playerNames.Add(user.Username);
        }

        private void OnUserLeftLobby(ConnectedUserDTO user)
        {
            playerNames.Remove(user.Username);
        }

        private void OnGameStarted()
        {
            if (!isHost)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    GoToGameAsync()
                );
            }
        }

        private async void GoToGameAsync()
        {
            await _navigationService.NavigateToPage($"{nameof(GamePage)}?LobbyId={LobbyId}");
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
            await _navigationService.NavigateBack();
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
                
                await _lobbyService.LeaveLobbyAsync(lobbyId);
                await _navigationService.NavigateBack();
            }
        }

        [RelayCommand]
        async Task GoToGame()
        {
            var res = await _lobbyService.StartGameAsync(LobbyId);
            if (res.Success)
            {
                await _navigationService.NavigateToPage($"{nameof(GamePage)}?LobbyId={LobbyId}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Failed", "to join lobby", "OK");
            }
        }

        //Handle result of different functions, and error log if neccesary:
        private void HandleActionResult(ActionResult message)
        {
            if (!message.Success)
            {
                // Give the user feedback about the error
                Debug.WriteLine("Creating a lobby failed: msg:", message.Msg);
            }
            else
            {
                Debug.WriteLine("This is your lobby id:", message.Msg);
            }
        }
    }
}
