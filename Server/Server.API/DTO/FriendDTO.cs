using Server.API.Models;

namespace Server.API.DTO
{
    public class FriendDTO (string? name, DateTime since, bool pending)
    {
        public string? Name { get; set; } = name;
        public DateTime FriendsSince { get; set; } = since;
        public bool IsPending { get; set; } = pending;
        public static FriendDTO FormFriendship(string userName, Friendship f)
        {
            var friendName = f.User1.UserName == userName ? f.User2.UserName : f.User1.UserName;
            return new FriendDTO(friendName, f.date, f.Status == "Pending"); ;
        }
    }
}
