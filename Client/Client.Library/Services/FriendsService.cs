﻿using Microsoft.Extensions.Configuration;
using Client.Library.DTO;
using Client.Library.Models;

namespace Client.Library.Services
{
    public class FriendsService : ConnectionService, IFriendsService
    {
        public event Action<FriendDTO>? NewFriendRequestEvent;
        public event Action<FriendDTO>? FriendRequestAcceptedEvent;
        public event Action<string, string>? NewGameInviteEvent;
        public event Action<string>? FriendRemovedEvent;

        public FriendsService(IConfiguration configuration) : base(configuration["ConnectionSettings:ApiUrl"] + configuration["ConnectionSettings:FriendsEndpoint"])
        {
            On<FriendDTO>("NewFriendRequest", (username) =>
                NewFriendRequestEvent?.Invoke(username));

            On<FriendDTO>("FriendRequestAccepted", (username) =>
                FriendRequestAcceptedEvent?.Invoke(username));

            On<string, string>("NewGameInvite", (username, lobbyId) =>
                NewGameInviteEvent?.Invoke(username, lobbyId));

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
