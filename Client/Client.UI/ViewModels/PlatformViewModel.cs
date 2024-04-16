using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Client.UI.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Client.UI.Services;

namespace Client.UI.ViewModels
{
    public partial class PlatformViewModel : ObservableObject
    {
        [ObservableProperty]
        string _username;
        [ObservableProperty]
        private bool gamesShowing = false;
        [ObservableProperty]
        private bool showhost = true;
        [ObservableProperty]
        private string _avatar;

        private readonly LobbyService _lobbyService;

        private ObservableCollection<Game> games;
        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        private int gameCounter = 0;
        public PlatformViewModel(LobbyService lobbyService)
        {
            _username = User.Instance.Username;
            _avatar = User.Instance.avatar;
            InitializeGame();
            _lobbyService = lobbyService;
        }

        private void InitializeGame()
        {
            games = new ObservableCollection<Game>();
            games.Add(new Game { Name = "hangman.png", Playable = false});
            games.Add(new Game { Name = "krydsogbolle.png", Playable = false });

            foreach (var game in games)
            {
                game.Playable=GameCheck(game.Name);
            }
        }

        public bool GameCheck(string s)
        {
            return User.Instance.games.Contains(s);
        }

        [RelayCommand]
        private async Task ChangeView()
        {
            GamesShowing = !GamesShowing;
            Showhost = !Showhost;
        }

        [RelayCommand]
        public async Task LogOut()
        {
            Preferences.Clear("auth_token");
            await Shell.Current.GoToAsync("LoginPage");
        }

        [RelayCommand]
        public async Task GoToLobby(string s)
        {
            var response= await _lobbyService.CreateLobbyAsync();
            if (response.Success)
            {
                await Shell.Current.GoToAsync($"LobbyPage?Image={s}&LobbyId={response.Msg}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Kunne ikke oprette lobby", "OK");
            }
        }
        [RelayCommand]
         public async Task GoToJoin(string s)
        {
            await Shell.Current.GoToAsync($"JoinPage");
        }
        
        public partial class Game: ObservableObject
        {
            [ObservableProperty]
            private string _name;
            [ObservableProperty]
            private bool _playable;
        }
    }
}

