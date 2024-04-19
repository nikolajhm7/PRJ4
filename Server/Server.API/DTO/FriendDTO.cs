using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO
    {
        public string name { get; set; }
        public DateTime friendsSince { get; set; }

        public static FriendDTO FromFriendship(string userId, Friendship f)
        {
            var friendId = f.User1Id == userId ? f.User2Id : f.User1Id;
            FriendDTO friendDTO = new FriendDTO
            {
                name = friendId,
                friendsSince = f.date
            };
            return friendDTO;
        }
    }
}
