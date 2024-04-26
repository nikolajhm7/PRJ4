using Microsoft.Extensions.Configuration;
using Client.Library.DTO;
using Client.Library.Models;

namespace Client.Library.Services
{
    public class FriendsService : ConnectionService, IFriendsService
    {
        public event Action<string>? NewFriendRequestEvent;
        public event Action<string>? FriendRequestAcceptedEvent;
        public event Action<string>? NewGameInviteEvent;
        public event Action<string>? FriendRemovedEvent;

        public FriendsService(IConfiguration configuration) : base(configuration["ConnectionSettings:ApiUrl"] + configuration["ConnectionSettings:FriendsEndpoint"])
        {
            On<string>("NewFriendRequest", (username) =>
                NewFriendRequestEvent?.Invoke(username));

            On<string>("FriendRequestAccepted", (username) =>
                FriendRequestAcceptedEvent?.Invoke(username));

            On<string>("NewGameInvite", (username) =>
                NewGameInviteEvent?.Invoke(username));

            On<string>("FriendRemoved", (username) =>
                FriendRemovedEvent?.Invoke(username));
        }

        public async Task<ActionResult> SendFriendRequest(string username)
        {
            return await InvokeAsync("SendFriendRequest", username);
        }

        public async Task<ActionResult> AcceptFriendRequest(string username)
        {
            return await InvokeAsync("AcceptFriendRequest", username);
        }

        public async Task<ActionResult> RemoveFriend(string username)
        {
            return await InvokeAsync("RemoveFriend", username);
        }

        public async Task<ActionResult> InviteFriend(string username)
        {
            return await InvokeAsync("InviteFriend", username);
        }

        public async Task<ActionResult<List<FriendDTO>>> GetFriends(bool getInvites)
        {
            return await InvokeAsync<List<FriendDTO>>("GetFriends", getInvites);
        }
    }
}
