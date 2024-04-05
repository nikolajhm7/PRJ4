using Microsoft.AspNetCore.Authorization;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace Server.API.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        public record ActionResult(bool Result, string? Msg);
        public async Task<ActionResult> SendFriendRequest(string username)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Clients.User(username).SendAsync("NewFriendRequest", Context.User.Identity.Name);

                // TODO: Store friend request in database

                return new ActionResult(true, null);
            }
            else
            {
                return new ActionResult(false, "User not authenticated.");
            }
        }

        public async Task<ActionResult> AcceptFriendRequest(string username)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Clients.User(username).SendAsync("FriendRequestAccepted", Context.User.Identity.Name);

                // TODO: Remove friend in database

                return new ActionResult(true, null);
            }
            else
            {
                return new ActionResult(false, "User not authenticated.");
            }
        }

        public async Task<ActionResult> RemoveFriend(string username)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Clients.User(username).SendAsync("FriendRemoved", Context.User.Identity.Name);

                // TODO: Remove friend in database

                return new ActionResult(true, null);
            }
            else
            {
                return new ActionResult(false, "User not authenticated.");
            }
        }

        public async Task<ActionResult> InviteFriend(string username)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Clients.User(username).SendAsync("NewGameInvite", Context.User.Identity.Name);
                return new ActionResult(true, null);
            }
            else
            {
                return new ActionResult(false, "User not authenticated.");
            }
        }
    }
}
