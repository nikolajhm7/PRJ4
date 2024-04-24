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


namespace Client.UI.ViewModels
{
    [QueryProperty(nameof(LobbyId), "lobbyId")]
    //[QueryProperty(nameof(Image), "image")]
    //[QueryProperty(nameof(Name), "name")]
    //[QueryProperty(nameof(GameId), "gameId")]
    [QueryProperty(nameof(Recivedgame), "game")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;

        [ObservableProperty] private string? lobbyId;
        //[ObservableProperty] private string? image;
        //[ObservableProperty] private string? name;
        //[ObservableProperty] private int? gameId;
        [ObservableProperty] private Game? recivedgame;

        [ObservableProperty] private Lobby lobby = new Lobby();

        public LobbyViewModel(ILobbyService lobbyService, INavigationService navigationService)
        {
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            Recivedgame = null;
            LobbyId = "00000";
           
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
