using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.UI.Models;
using Client.UI.Services;
using System.Diagnostics;
using Client.UI.DTO;
using Client.UI.Services;



namespace Client.UI.ViewModels
{
    [QueryProperty("Image", "Image")]
    [QueryProperty("LobbyId", "LobbyId")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly LobbyService _lobbyService;
        private readonly NavigationService _navigationService;

        [ObservableProperty]
        private int lobbyId = 123; //test

        [ObservableProperty]
        private string image;

        [ObservableProperty]
        private Lobby lobby = new Lobby();

        public LobbyViewModel(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
            _navigationService = new NavigationService();

            Debug.WriteLine("Initializing lobby");

            // Example initialization, replace with actual dynamic data loading
            Lobby.PlayerNames.Add("Player 1");
            Lobby.PlayerNames.Add("Player 2");
            Lobby.PlayerNames.Add("Player 3"); // Example Lobby ID

            // Subscribe to events
            _lobbyService.UserJoinedLobbyEvent += OnUserJoinedLobby;
            _lobbyService.UserLeftLobbyEvent += OnUserLeftLobby;
        }

        private void OnUserJoinedLobby(ConnectedUserDTO user)
        {
            Lobby.PlayerNames.Add(user.Username);
        }

        private void OnUserLeftLobby(ConnectedUserDTO user)
        {
            Lobby.PlayerNames.Remove(user.Username);
        }


        [RelayCommand]
        public async Task TestAddPlayer()
        {
            Lobby.PlayerNames.Add("Player 4");
        }

        [RelayCommand]
        async Task GoBack()
        {
            _navigationService.NavigateBack();
        }

        //Handle result of different functions, and error log if neccesary:
        private void HandleActionResult(ConnectionService.ActionResult message)
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
