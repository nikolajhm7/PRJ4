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
using Client.Library.Services;
using System.Diagnostics;
using Client.Library.Services.Interfaces;
using Client.UI.Views;
using Client.Library.DTO;

namespace Client.UI.ViewModels
{
    public partial class JoinViewModel : ObservableObject
    {
        private readonly ILobbyService _lobbyService;
        private readonly INavigationService _navigationService;
        private IJwtTokenService _jwtTokenService;
        
        [ObservableProperty]
        private string _lobbyId;
        
        public JoinViewModel(ILobbyService lobbyService, INavigationService navigationService, IJwtTokenService jwtTokenService)
        { 
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _jwtTokenService = jwtTokenService;
        }
        [RelayCommand]
       public async Task GoToLobby()
       {
            var response = await _lobbyService.JoinLobbyAsync(_lobbyId);

            if (response.Success)
            {
                await _navigationService.NavigateToPage($"{nameof(LobbyPage)}?LobbyId={response.Msg}");
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
                
                if(!string.IsNullOrEmpty(authToken) && !_jwtTokenService.IsUserRoleGuest())
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