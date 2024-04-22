﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Libary.Models;
using System.Diagnostics;
using Client.Libary.DTO;
using Client.Libary.Services;



namespace Client.UI.ViewModels
{
    [QueryProperty("Image", "Image")]
    [QueryProperty("LobbyId", "LobbyId")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly NavigationService _navigationService;

        [ObservableProperty]
        private string ?lobbyId;

        [ObservableProperty]
        private string ?image;

        [ObservableProperty]
        private Lobby lobby = new Lobby();

        public LobbyViewModel(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService;
            _navigationService = new NavigationService();

            // Example initialization, replace with actual dynamic data loading
            if(lobbyId != null)
                Lobby.LobbyId = lobbyId; // Example Lobby ID
            else
                Lobby.LobbyId = "00000"; // Example Lobby ID
            System.Console.WriteLine(Image);

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
            await _navigationService.NavigateBack();
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
