using Microsoft.AspNetCore.Authorization;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.SignalR;
using Server.API.Data;
using Server.API.Repositories.Interfaces;
using Server.API.Repositories;
using Server.API.Services.Interfaces;

namespace Server.API.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        private readonly ILogger<FriendsHub> _logger;
        private readonly IFriendsRepository _friendsRepository;
        private readonly ILobbyManager _lobbyManager;

        public FriendsHub(ILogger<FriendsHub> logger, IFriendsRepository friendsRepository, ILobbyManager lobbyManager)
        {
            _logger = logger;
            _friendsRepository = friendsRepository;
            _lobbyManager = lobbyManager;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var username = Context.User?.Identity?.Name;
            if (username != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
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

            await Clients.Group(otherUsername).SendAsync("NewFriendRequest", new FriendDTO(username, DateTime.Now, true));

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

            await Clients.Group(otherUsername).SendAsync("FriendRequestAccepted", new FriendDTO(username, DateTime.Now, false));

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

            await Clients.Group(otherUsername).SendAsync("FriendRemoved", username);

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

            if (await _friendsRepository.FindFriendship(username, otherUsername) == null)
            {
                _logger.LogWarning("{User} is not friends with {Friend}.", username, otherUsername);
                return new ActionResult(false, "You can only invite friends to a game.");
            }

            var lobbyId = _lobbyManager.GetLobbyIdFromUsername(username);
            if (lobbyId == null)
            {
                _logger.LogWarning("Couldn't find a lobby for {username}.", username);
                return new ActionResult(false, "You must be in a lobby to invite friends to a game.");
            }

            _logger.LogInformation("{User} sent a game invite to {Friend}.", username, otherUsername);

            await Clients.Group(otherUsername).SendAsync("NewGameInvite", username, lobbyId);
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

            var friends = await _friendsRepository.GetFriendsOf(username);

            if (getInvites)
            {
                friends.AddRange(await _friendsRepository.GetInvitesOf(username));
            }

            return new ActionResult<List<FriendDTO>>(true, null, friends);
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            var username = Context.User?.Identity?.Name;
            if (username != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);

            await base.OnDisconnectedAsync(e);
        }
    }
}
