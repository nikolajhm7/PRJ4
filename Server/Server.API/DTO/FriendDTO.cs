using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO
    {
        public string? Name { get; set; }
        public DateTime FriendsSince { get; set; }

        public bool IsAccepted { get; set; }
        public static FriendDTO FromFriendship(string userId, Friendship f)
        {
            var friendId = f.User1Id == userId ? f.User2Id : f.User1Id;
            FriendDTO friendDTO = new FriendDTO
            {
                Name = friendId,
                FriendsSince = f.date,
                IsAccepted = f.Status == "Accepted"
            };
            return friendDTO;
        }
    }
}
