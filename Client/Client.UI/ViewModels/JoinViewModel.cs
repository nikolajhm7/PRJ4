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
using Client.UI.Views;

namespace Client.UI.ViewModels
{
    public partial class JoinViewModel : ObservableObject
    {
        private readonly LobbyService _lobbyService;
        private readonly NavigationService _navigationService;
        [ObservableProperty]
        private string _lobbyId;
        public JoinViewModel(LobbyService lobbyService)
        { 
            _lobbyService = lobbyService;
            _navigationService = new NavigationService();
        }
        [RelayCommand]
       public async Task GoToLobby()
        {
            var result = await _lobbyService.JoinLobbyAsync(_lobbyId);
            if (result.Success)
            {
                await Shell.Current.GoToAsync($"//LobbyPage?LobbyId={_lobbyId}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Failed", "to join lobby", "OK");
            }
        }
        [RelayCommand]
        public async Task GoBack()
        {
                string authToken = Preferences.Get("auth_token", defaultValue: string.Empty);
                if(!string.IsNullOrEmpty(authToken))
                {
                    await _navigationService.NavigateToPage(nameof(PlatformPage));
                }
                else
                {
                    await _navigationService.NavigateToPage("//"+nameof(LoginPage));
                }
        }
    }
}