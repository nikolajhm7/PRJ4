using Client.Library.DTO;
using Client.Library.Services.Interfaces;
using Client.Library.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Library.Models;
using Client.UI.Views;
using Microsoft.Extensions.Logging;

namespace Client.UI.ViewModels
{
    public partial class FriendsViewModel : ObservableObject
    {
        #region Properties

        [ObservableProperty]
        private bool _isUser = true;

        [ObservableProperty]
        private bool _canInvite;

        [ObservableProperty]
        private string _addFriendText;

        private ObservableCollection<FriendDTO> friendsCollection = [];
        public ObservableCollection<FriendDTO> FriendsCollection
        {
            get { return friendsCollection; }
            set { SetProperty(ref friendsCollection, value); }
        }

        #endregion

        #region Interfaces

        private readonly IFriendsService _friendsService;

        private readonly ILobbyService _lobbyService;

        private readonly INavigationService _navigationService;
        private readonly IJwtTokenService _jwtTokenService;
        
        private ILogger<FriendsViewModel> _logger;

        #endregion

        public FriendsViewModel(IFriendsService friendsService, ILobbyService lobbyService, INavigationService navigationService, IJwtTokenService jwtTokenService, ILogger<FriendsViewModel> logger)
        {
            _friendsService = friendsService;
            _lobbyService = lobbyService;
            _navigationService = navigationService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;

            _friendsService.NewFriendRequestEvent += OnNewFriendRequest;
            _friendsService.FriendRequestAcceptedEvent += OnFriendRequestAccepted;
            _friendsService.FriendRemovedEvent += OnFriendRemoved;
            _friendsService.NewGameInviteEvent += OnNewGameInvite;
        }

        public async void OnLoaded(object? e, EventArgs args)
        {
            IsUser = !_jwtTokenService.IsUserRoleGuest();
            if (IsUser)
            {
                _logger.LogInformation("User is not a guest, retrieving friends.");
                await RetrieveFriends();
            }
            else
            {
                _logger.LogInformation("User is a guest, skipping retrieval of friends.");
            }
        }

        #region Friends
        [RelayCommand]
        public async Task RetrieveFriends()
        {
            _logger.LogInformation("Starting to retrieve friends.");
            ActionResult<List<FriendDTO>> res = await _friendsService.GetFriends(true);
            if (res.Success)
            {

                foreach (var friendDTO in res.Value)
                {
                    if (FriendsCollection.Any(f => f.Name == friendDTO.Name))
                    { }
                    else
                    {
                        var temp = friendDTO;
                        FriendsCollection.Add(temp);
                    }
                }
                _logger.LogInformation("Successfully retrieved and updated friends list.");
            }
            else
            {
                _logger.LogError("Failed to retrieve friends.");
                await Shell.Current.DisplayAlert("Error", "Failed to get friends", "OK");
            }
        }

        [RelayCommand]
        public async Task AddNewFriend()
        {
            _logger.LogInformation("Attempting to add new friend: {Username}", AddFriendText);
            var text = AddFriendText;
            AddFriendText = string.Empty;

            var res = await _friendsService.SendFriendRequest(text);

            if (!res.Success)
            {
                _logger.LogError("Failed to send friend request to: {Username}.", text);
                await Shell.Current.DisplayAlert("Error", "Failed to send friend request", "OK");
            }
            else
            {
                _logger.LogInformation("Friend request sent to: {Username}", text);
            }
        }

        [RelayCommand]
        public async Task AcceptFriendRequest(string s)
        {
            _logger.LogInformation("Accepting friend request from: {Username}", s);
            await _friendsService.AcceptFriendRequest(s);
            var friend = FriendsCollection.FirstOrDefault(f => f.Name == s);
            if (friend != null)
            {
                friend.IsPending = false;
                _logger.LogInformation("Friend request accepted and updated for: {Username}", s);
            }
            else
            {
                _logger.LogWarning("Attempted to accept friend request, but user not found in list: {Username}", s);
            }
        }

        [RelayCommand]
        public async Task DeclineFriendRequest(string s)
        {
            _logger.LogInformation("Declining friend request from: {Username}", s);
            await _friendsService.RemoveFriend(s);
            FriendsCollection.Remove(FriendsCollection.FirstOrDefault(f => f.Name == s));
        }

        [RelayCommand]
        public async Task InviteFriend(string username)
        {
            await _friendsService.InviteFriend(username);
        }

        public void OnNewFriendRequest(FriendDTO user)
        {
            FriendsCollection.Add(user);
        }

        public void OnFriendRequestAccepted(FriendDTO user)
        {
            var oldUser = FriendsCollection.FirstOrDefault(u => u.Name == user.Name);
            if (oldUser != null)
            {
                FriendsCollection.Remove(oldUser);
                FriendsCollection.Add(user);
            }
        }

        public void OnFriendRemoved(string username)
        {
            var user = FriendsCollection.FirstOrDefault(u => u.Name == username);
            if (user != null) FriendsCollection.Remove(user);
        }

        public void OnNewGameInvite(string username, string lobbyId)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HandleInvite(username, lobbyId);
            });
        }

        public async void HandleInvite(string username, string lobbyId)
        {
            var res = await Shell.Current.DisplayAlert("Game Invite", $"{username} has invited you to their lobby: {lobbyId}", "Join", "Cancel");

            if (!res) return;

            var response = await _lobbyService.JoinLobbyAsync(lobbyId);

            if (response.Success)
            {
                await _navigationService.NavigateToPage($"{nameof(LobbyPage)}?LobbyId={response.Msg}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Failed", "to join lobby", "OK");
            }
        }
        #endregion
    }
}
