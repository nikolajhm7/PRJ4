using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO
    {
        public string? FriendId { get; set; }
        public DateTime FriendsSince { get; set; }

        public bool IsAccepted { get; set; }
        public static FriendDTO FormFriendship(string userName, Friendship f)
        {
            var friendId = f.User1.UserName == userName ? f.User2.UserName : f.User1.UserName;
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
