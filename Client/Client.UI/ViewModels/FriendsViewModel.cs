using Client.Library.DTO;
using Client.Library.Services.Interfaces;
using Client.Library.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.Library.Models;
using Client.UI.Views;

namespace Client.UI.ViewModels
{
    public partial class FriendsViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private string _labelText = "WAITING";

        //[RelayCommand]
        //public async Task ButtonPressed()
        //{
        //    LabelText = "PRESSED!";
        //    await Task.Delay(2000);
        //    LabelText = "WAITING";
        //}
        #region Properties

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

        #endregion

        public FriendsViewModel(IFriendsService friendsService, ILobbyService lobbyService, INavigationService navigationService)
        {
            _friendsService = friendsService;
            _lobbyService = lobbyService;
            _navigationService = navigationService;

            _friendsService.NewFriendRequestEvent += OnNewFriendRequest;
            _friendsService.FriendRequestAcceptedEvent += OnFriendRequestAccepted;
            _friendsService.FriendRemovedEvent += OnFriendRemoved;
            _friendsService.NewGameInviteEvent += OnNewGameInvite;
        }

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
                    { }
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
