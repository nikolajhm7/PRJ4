using Server.API.Models;

namespace Server.API.Models
{
    public class Friendship
    {
        public string User1Id { get; set; }
        public User User1 { get; set; }
        public string User2Id { get; set; }
        public User User2 { get; set; }
        public int Status { get; set; }

        public DateTime date;
    }
}
