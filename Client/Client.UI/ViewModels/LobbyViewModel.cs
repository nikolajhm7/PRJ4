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



namespace Client.UI.ViewModels
{
    [QueryProperty("Image", "Image")]
    public partial class LobbyViewModel : ObservableObject
    {
        private readonly LobbyService lobbyService;

        [ObservableProperty]
        private string image;

        [ObservableProperty]
        private Lobby lobby = new Lobby();

        public LobbyViewModel(LobbyService lobbyService)
        {
            this.lobbyService = lobbyService;
            
            
            // Example initialization, replace with actual dynamic data loading
            Lobby.PlayerNames.Add("Player 1");
            Lobby.PlayerNames.Add("Player 2");
            Lobby.PlayerNames.Add("Player 3");
            Lobby.LobbyId = "12345"; // Example Lobby ID
            Console.WriteLine(Image);
        }

        [RelayCommand]
        public async Task TestAddPlayer()
        {
            Lobby.PlayerNames.Add("Player 4");
        }

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
