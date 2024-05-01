using System.Collections.ObjectModel;
using Client.Library.Interfaces;
using Client.Library.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Client.UI.Managers;
using Client.UI.Views;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Client.UI.ViewModels
{
    public partial class PlatformViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _username;
        [ObservableProperty] 
        private bool _gamesShowing = false;
        [ObservableProperty] 
        private bool _showhost = true;
        [ObservableProperty] 
        private string _avatar;

        private readonly ILobbyService _lobbyService;

        private readonly INavigationService _navigationService;

        private readonly HttpClient _httpClient;

        private readonly IApiService _apiService;

        private readonly IConfiguration _configuration;

        private IJwtTokenService _jwtTokenService;

        private IPreferenceManager _preferenceManager;

        private ObservableCollection<Game> games=[];
        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        public PlatformViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService, ILobbyService lobbyService, IPreferenceManager preferenceManager, IJwtTokenService jwtTokenService, IApiService apiService)
        {
            _configuration = configuration;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _preferenceManager = preferenceManager;
            _jwtTokenService = jwtTokenService;
            _apiService = apiService;
        }

        public void OnPageAppearing()
        {
            Username = _jwtTokenService.GetUsernameFromToken();
            SetAvatar();
            pullGames();
        }
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
            await _navigationService.NavigateToPage(nameof(LoginPage));
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
    }
}

