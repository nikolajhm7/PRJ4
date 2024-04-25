using Microsoft.AspNetCore.Authorization;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.SignalR;
using Server.API.Data;
using Server.API.Repositories.Interfaces;
using Server.API.Repositories;

namespace Server.API.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        private readonly ILogger<FriendsHub> _logger;
        private readonly IFriendsRepository _friendsRepository;

        public FriendsHub(ILogger<FriendsHub> logger, IFriendsRepository friendsRepository)
        {
            _logger = logger;
            _friendsRepository = friendsRepository;
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

            await _friendsRepository.AddFriendRequest(username, otherUsername);

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

            await _friendsRepository.AcceptFriendRequest(username, otherUsername);

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

            _logger.LogInformation("{User} removed {Friend} from friends list.", username, otherUsername);

            await Clients.User(otherUsername).SendAsync("FriendRemoved",username);

            await _friendsRepository.RemoveFriend(username, otherUsername);

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

        public async Task<ActionResult<List<FriendDTO>>> GetFriends(bool getInvites)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                _logger.LogWarning("Context.User or Context.User.Identity is null.");
                return new ActionResult<List<FriendDTO>>(false, "Authentication context is not available.", []);
            }

            _logger.LogInformation("Requesting all friends of {User}.", username);

            var friends = await _friendsRepository.GetFriendsOf(username, getInvites);
            return new ActionResult<List<FriendDTO>>(true, null, friends);
        }
    }
}
