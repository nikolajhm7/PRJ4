using Microsoft.AspNetCore.Authorization;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.SignalR;
using Server.API.Data;

namespace Server.API.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        public record ActionResult(bool Success, string? Msg);

        private readonly ILogger<FriendsHub> _logger;

        public FriendsHub(ILogger<FriendsHub> logger)
        {
            _logger = logger;
        }
        public async Task<ActionResult> SendFriendRequest(string otherUsername)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogInformation("Sending friend request from {Requestor} to {Requested}.", username, otherUsername);

            await Clients.User(otherUsername).SendAsync("NewFriendRequest", username);

            // TODO: Store friend request in database

            return new ActionResult(true, null);
        }

        public async Task<ActionResult> AcceptFriendRequest(string otherUsername)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogInformation("{User} accepted a friend request from {Requestor}.", username, otherUsername);

            await Clients.User(otherUsername).SendAsync("FriendRequestAccepted", username);

            // TODO: Remove friend in database

            return new ActionResult(true, null);
        }

        public async Task<ActionResult> RemoveFriend(string otherUsername)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogInformation("{User} removed {Friend} from friendslist.", username, otherUsername);

            await Clients.User(otherUsername).SendAsync("FriendRemoved",username);

            // TODO: Remove friend in database

            return new ActionResult(true, null);
        }

        public async Task<ActionResult> InviteFriend(string otherUsername)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult(false, "Authentication context is not available.");
            }

            _logger.LogInformation("{User} sent a game invite to {Friend}.", username, otherUsername);

            await Clients.User(otherUsername).SendAsync("NewGameInvite", username);
            return new ActionResult(true, null);
        }
    }
}
