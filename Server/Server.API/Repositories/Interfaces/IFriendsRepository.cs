using Server.API.DTO;

namespace Server.API.Repositories.Interfaces
{
    public interface IFriendsRepository
    {
        Task AddFriendRequest(string userId, string friendId);
        Task AcceptFriendRequest(string userId, string friendId);
        Task RemoveFriend(string userId, string friendId);
        Task<List<FriendDTO>> GetFriendsOf(string userId, bool getInvites);
    }
}
