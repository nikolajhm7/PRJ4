using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Library.DTO;
using Client.Library.Models;

namespace Client.Library.Services
{
    public interface IFriendsService
    {
        public event Action<string>? NewFriendRequestEvent;
        public event Action<string>? FriendRequestAcceptedEvent;
        public event Action<string>? NewGameInviteEvent;
        public event Action<string>? FriendRemovedEvent;

        Task<ActionResult> SendFriendRequest(string username);

        Task<ActionResult> AcceptFriendRequest(string username);

        Task<ActionResult> RemoveFriend(string username);

        Task<ActionResult> InviteFriend(string username);

        Task<ActionResult<List<FriendDTO>>> GetFriends(bool getInvites);
    }
}
