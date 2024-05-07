using Server.API.DTO;
using Server.API.Models;

namespace Server.API.Repositories.Interfaces
{
    public interface IFriendsRepository
    {
        Task AddFriendRequest(string username, string friendName);
        Task AcceptFriendRequest(string username, string friendName);
        Task RemoveFriend(string username, string friendName);
        Task<Friendship?> FindFriendship(string username, string friendName);
        Task<List<FriendDTO>> GetFriendsOf(string username);
        Task<List<FriendDTO>> GetInvitesOf(string userName);
    }
}
