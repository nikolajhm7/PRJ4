using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO
    {
        public string? FriendId { get; set; }
        public DateTime FriendsSince { get; set; }

        public bool IsAccepted { get; set; }
        public static FriendDTO FromFriendship(string userId, Friendship f)
        {
            var friendId = f.User1Id == userId ? f.User2Id : f.User1Id;
            FriendDTO friendDTO = new FriendDTO
            {
                FriendId = friendId,
                FriendsSince = f.date,
                IsAccepted = f.Status == "Accepted"
            };
            return friendDTO;
        }
    }
}
