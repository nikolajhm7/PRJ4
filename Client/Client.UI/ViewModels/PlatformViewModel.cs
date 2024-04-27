using System.Collections.ObjectModel;
using Client.Library.DTO;
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

        private IFriendsService _friendService;

        private ObservableCollection<string> friends;

        public ObservableCollection<string> Friends
        {
            get { return friends; }
            set { SetProperty(ref friends, value); }
        }


        private ObservableCollection<Game> games;
        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        public PlatformViewModel( IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService, ILobbyService lobbyService, IPreferenceManager preferenceManager, IJwtTokenService jwtTokenService, IApiService apiService)
        {
            _configuration = configuration;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _preferenceManager = preferenceManager;
            _jwtTokenService = jwtTokenService;
            _apiService = apiService;
            //SetAvatar();
            //pullGames();
        }

        public void OnPageAppearing()
        {
            SetAvatar();
            pullGames();
            GetFriendList();
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
            string endpoint = $"/Game/getGamesForUser/{User.Instance.Username}";
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
            switch (UserInstance.Avatar)
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
        async Task GoToLobby(Game s)
        {
            if (s != null)
            {
                int someint = 1;
                var response = await _lobbyService.CreateLobbyAsync(someint);
                if (response.Success)
                {
                    var box = new Dictionary<string, object>
                    {
                        //{"gameId", s.GameId},
                        //{"name", s.Name},
                        //{"image", s.Image},
                        { "lobbyId", response.Msg },
                        { "game", s }
                    };
                    await Shell.Current.GoToAsync(nameof(LobbyPage), true, box);
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
        async Task Addfriend(string s)
        {
            await _friendService.SendFriendRequest(s);
        }
        [RelayCommand]
        async Task RemoveFriends(string s)
        {
            await _friendService.RemoveFriend(s);
        }

        [RelayCommand]
        async Task AcceptFriend(string s)
        {
            await _friendService.AcceptFriendRequest(s);
        }
        async Task GetFriendList()
        {
            ActionResult<List<FriendDTO>> temp = await _friendService.GetFriends(true);
            if (temp.Success)
            {
                foreach (var friendDTO in temp.Value)
                {
                    Friends.Add(friendDTO.Name);
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to get friends", "OK");
            }
        }
    }
}

