﻿using System.Collections.ObjectModel;
using Client.Library.DTO;
using Client.Library.Interfaces;
using Client.Library.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Client.UI.Views;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Client.UI.ViewModels
{
    public partial class PlatformViewModel : ObservableObject
    {
        #region Properties

        [ObservableProperty] private string? _username;
        [ObservableProperty] private bool _gamesShowing = false;
        [ObservableProperty] private bool _showhost = true;
        [ObservableProperty] private string _avatar;

        private ObservableCollection<Game> games;

        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        #endregion

        #region Interfaces

        private readonly ILobbyService _lobbyService;

        private readonly INavigationService _navigationService;

        private readonly IApiService _apiService;

        private readonly IConfiguration _configuration;

        private IJwtTokenService _jwtTokenService;

        private IPreferenceManager _preferenceManager;

        private readonly IFriendsService _friendsService;
        #endregion

        public PlatformViewModel(IFriendsService friendsService, IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService, ILobbyService lobbyService, IPreferenceManager preferenceManager, IJwtTokenService jwtTokenService, IApiService apiService)
        {
            _configuration = configuration;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _preferenceManager = preferenceManager;
            _jwtTokenService = jwtTokenService;
            _apiService = apiService;
            _friendsService = friendsService;
        }

        public async void OnPageAppearing()
        {
            Username = _jwtTokenService.GetUsernameFromToken();
            SetAvatar();
            await pullGames();
        }

        #region Setting up frontend stuff

        public void SetImagesForGames()
        {
            foreach (var game in Games)
            {
                game.setImage();
            }
        }

        [RelayCommand]
        public async Task pullGames()
        {
            string endpoint = $"/Game/getGamesForUser/{_jwtTokenService.GetUsernameFromToken()}";
            var response = await _apiService.MakeApiCall(endpoint, HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(jsonResponse);
                SetImagesForGames();
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Kunne ikke hente spil", "OK");
            }

        }
        public void SetAvatar()
        {
            int i = 0;
            Random random = new Random();
            i = random.Next(1, 4);
            switch (i)
            {
                case 1:
                    Avatar = "charizard.png";
                    break;
                case 2:
                    Avatar = "pikachu.png";
                    break;
                case 3:
                    Avatar = "mewtow.png";
                    break;
            }
        }

        #endregion

        #region Navigating
        [RelayCommand]
        private async Task ChangeView()
        {
            GamesShowing = !GamesShowing;
            Showhost = !Showhost;
        }

        [RelayCommand]
        async Task LogOut()
        {
            _preferenceManager.Remove("auth_token");
            _preferenceManager.Remove("refresh_token");
            await _friendsService.DisconnectAsync();
            await _lobbyService.DisconnectAsync();
            await _navigationService.NavigateBackToPage(nameof(LoginPage));
        }

        [RelayCommand]
        async Task GoToSettings()
        {
            await _navigationService.NavigateToPage(nameof(SettingsPage));
        }

        [RelayCommand]
        public async Task GoToLobby(Game s)
        {
            if (s != null)
            {
                var response = await _lobbyService.CreateLobbyAsync(s.GameId);

                if (response.Success)
                {
                    await _navigationService.NavigateToPage($"{nameof(LobbyPage)}?LobbyId={response.Msg}");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Fejl", "Kunne ikke oprette lobby", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Vælg et spil som du har adgang til", "OK");
            }
        }

        [RelayCommand]
        async Task GoToJoin(string s)
        {
            await _navigationService.NavigateToPage(nameof(JoinPage));
        }

        [RelayCommand]
        async Task GoToShop()
        {
            await _navigationService.NavigateToPage(nameof(ShopPage));
        }
        #endregion
        
    }
}

