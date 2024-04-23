﻿using System.Collections.ObjectModel;
using Client.Library.Interfaces;
using Client.Library.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Client.UI.Managers;
using Client.UI.Views;

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

        private readonly ILobbyService _lobbyService;

        private readonly INavigationService _navigationService;

        private ObservableCollection<Game> games;
        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        private IPreferenceManager _preferenceManager;

        private int gameCounter = 0;
        public PlatformViewModel(ILobbyService lobbyService, IPreferenceManager preferenceManager, INavigationService navigationService)
        {
            _username = User.Instance.Username;
            _avatar = User.Instance.avatar;
            InitializeGame();
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _preferenceManager = preferenceManager;
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
        async Task GoToLobby(string s)
        {
            int someint = 1;
            var response = await _lobbyService.CreateLobbyAsync(someint);
            if (response.Success)
            {
                await Shell.Current.GoToAsync($"//LobbyPage?Image={s}&LobbyId={response.Msg}");
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
        
        public partial class Game: ObservableObject
        {
            [ObservableProperty]
            private string _name;
            [ObservableProperty]
            private bool _playable;
        }
    }
}

