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
        public User UserInstance => User.Instance;
        [ObservableProperty]
        private bool gamesShowing = false;
        [ObservableProperty]
        private bool showhost = true;
        [ObservableProperty]
        private string _avatar;

        private readonly ILobbyService _lobbyService;

        private readonly INavigationService _navigationService;

        private readonly HttpClient _httpClient;

        private readonly IApiService _apiService;

        private readonly IConfiguration _configuration;

        private IJwtTokenService _jwtTokenService;

        private IPreferenceManager _preferenceManager;

        private ObservableCollection<Game> games;
        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }


        private int gameCounter = 0;
        public PlatformViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService, ILobbyService lobbyService, IPreferenceManager preferenceManager, IJwtTokenService jwtTokenService, IApiService apiService)
        {
            _configuration = configuration;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _preferenceManager = preferenceManager;
            _jwtTokenService = jwtTokenService;
            _apiService = apiService;
            SetAvatar();
            pullGames();
        }

        public void SetImagesForGames()
        {
            foreach (var game in Games)
            {
                game.setImage(); // Assuming setimage() is a method in the Game class
            }
        }

        [RelayCommand]
        public async Task pullGames()
        {
            string endpoint = $"/games/Game/getGamesForUser/{User.Instance.Username}";
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
            switch (UserInstance.avatar)
            {
                case 1:
                    _avatar = "charizard.png";
                    break;
                case 2:
                    _avatar = "pikachu.png";
                    break;
                case 3:
                    _avatar = "mewtow.png";
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
            _preferenceManager.Clear("auth_token");
            await _navigationService.NavigateToPage("///"+nameof(LoginPage));
        }

        [RelayCommand]
        async Task GoToSettings()
        {
            await _navigationService.NavigateToPage(nameof(SettingsPage));
        }

        [RelayCommand]
        async Task GoToLobby(Game s)
        {
            int someint = 1;
            var response = await _lobbyService.CreateLobbyAsync(someint);
            if (response.Success)
            {
                var box = new Dictionary<string, object>
                {
                        {"gameId", s.GameId},
                        {"name", s.Name},
                        {"image", s.Image},
                        {"lobbyId",response.Msg},
                };
                await Shell.Current.GoToAsync("//LobbyPage",box);
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Kunne ikke oprette lobby", "OK");
            }
        }
        [RelayCommand]
        async Task GoToJoin(string s)
        {
            await _navigationService.NavigateToPage(nameof(JoinPage));
        }
    }
}

