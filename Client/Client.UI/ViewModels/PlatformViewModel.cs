using System.Collections.ObjectModel;
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
            [ObservableProperty]
        private string _addFriendText;
        [ObservableProperty]
        private string _addFriendPlaceholder;
        
        private ObservableCollection<FriendDTO> friendsCollection = [];
        public ObservableCollection<FriendDTO> FriendsCollection
        {
            get { return friendsCollection; }
            set { SetProperty(ref friendsCollection, value); }
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
            await RetrieveFriends();
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
        #endregion
        
        #region Friends
        [RelayCommand]
        public async Task RetrieveFriends()
        {
            
            ActionResult<List<FriendDTO>> res = await _friendsService.GetFriends(true);
                if (res.Success)
                {
                    
                foreach (var friendDTO in res.Value)
                    {
                        if (FriendsCollection.Any(f => f.Name == friendDTO.Name))
                        {}
                        else
                        {
                            var temp = friendDTO;
                            FriendsCollection.Add(temp);
                        }
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to get friends", "OK");
                }
        }

        [RelayCommand]
        public async Task AddNewFriend()
        {
            var text = AddFriendText;
            AddFriendText = string.Empty;

            var res = await _friendsService.SendFriendRequest(text);

            if (!res.Success)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to send friend request", "OK");
            }
        }

        [RelayCommand]
        public async Task AcceptFriendRequest(string s)
        {
            await _friendsService.AcceptFriendRequest(s);
             var friend = FriendsCollection.FirstOrDefault(f => f.Name == s);
            if (friend != null)
                {
                    friend.IsPending = false;
                }
            
        }

        [RelayCommand]
        public async Task DeclineFriendRequest(string s)
        {

           await _friendsService.RemoveFriend(s);
           FriendsCollection.Remove(FriendsCollection.FirstOrDefault(f => f.Name == s));
        }
        #endregion
    }
}

