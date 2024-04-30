using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO
    {
        public string? Name { get; set; }
        public DateTime FriendsSince { get; set; }

        public bool IsAccepted { get; set; }
        public static FriendDTO FormFriendship(string userName, Friendship f)
        {
            var friendName = f.User1.UserName == userName ? f.User2.UserName : f.User1.UserName;
            FriendDTO friendDTO = new FriendDTO
            {
                Name = friendName,
                FriendsSince = f.date,
                IsAccepted = f.Status == "Accepted"
            };
            return friendDTO;
        }
    }
}
