using Microsoft.AspNetCore.SignalR.Client;

namespace Client.UI.Services
{
    public class FriendsService
    {
        private readonly IConnectionService _connectionService;

        public event Action<string>? NewFriendRequestEvent;
        public event Action<string>? FriendRequestAcceptedEvent;
        public event Action<string>? NewGameInviteEvent;
        public event Action<string>? FriendRemovedEvent;

        public FriendsService(IConnectionService connectionService)
        {
            _connectionService = connectionService;

            _connectionService.On<string>("NewFriendRequest", (username) =>
                NewFriendRequestEvent?.Invoke(username));

            _connectionService.On<string>("FriendRequestAccepted", (username) =>
                FriendRequestAcceptedEvent?.Invoke(username));

            _connectionService.On<string>("NewGameInvite", (username) =>
                NewGameInviteEvent?.Invoke(username));

            _connectionService.On<string>("FriendRemoved", (username) =>
                FriendRemovedEvent?.Invoke(username));
        }

        public async Task<IConnectionService.ActionResult> SendFriendRequest(string username)
        {
            return await _connectionService.InvokeAsync("SendFriendRequest", username);
        }

        public async Task<IConnectionService.ActionResult> AcceptFriendRequest(string username)
        {
            return await _connectionService.InvokeAsync("AcceptFriendRequest", username);
        }

        public async Task<IConnectionService.ActionResult> RemoveFriend(string username)
        {
            return await _connectionService.InvokeAsync("RemoveFriend", username);
        }

        public async Task<IConnectionService.ActionResult> InviteFriend(string username)
        {
            return await _connectionService.InvokeAsync("InviteFriend", username);
        }
    }
}
