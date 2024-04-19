using Server.API.DTO;

namespace Server.API.Repositories.Interfaces
{
    public interface IFriendsRepository
    {
        void AddFriendRequest(string userId, string friendId);
        void AcceptFriendRequest(string userId, string friendId);
        void RemoveFriend(string userId, string friendId);
        List<FriendDTO> GetFriendsOf(string userId);
    }
}
