using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Library.Models;
using System.Diagnostics;
using Client.Library.DTO;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;


namespace Client.UI.ViewModels
{
    
    [QueryProperty(nameof(LobbyId), "lobbyId")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private Lobby _lobby;
        bool isHost = false;
        public string HostName { get; set; }

        [ObservableProperty] 
        private string image = "";

        // Observable properties
        [ObservableProperty] 
        private string lobbyId = "000000";

        [ObservableProperty]
        public ObservableCollection<string> playerNames = new ObservableCollection<string> { };

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
            var isHostResult = await _lobbyService.UserIsHost(lobbyId);
            if (isHostResult.Success)
            {
                isHost = true;
            }
            await LoadUsersInLobby();
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
            // Navigate to game page
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

        private async Task CloseLobby()
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
