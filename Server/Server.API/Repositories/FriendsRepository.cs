using Server.API.Data;
using Server.API.Repositories.Interfaces;
using Server.API.DTO;
using Server.API.Models;

namespace Server.API.Repositories
{
    public class FriendsRepository : IFriendsRepository
    {
        private readonly ApplicationDbContext _context;

        public FriendsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddFriendRequest(string userId, string friendId)
        {
            if(userId == friendId) return;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var friend = _context.Users.FirstOrDefault(u => u.Id == friendId);

            if(user == null || friend == null) return;

            var friendRequest = new Friendship
            {
                User1Id = user.Id,
                User1 = user,
                User2Id = friend.Id,
                User2 = friend,
                Status = "Pending",
                date = DateTime.Now
            };

            _context.Friendships.Add(friendRequest);
            _context.SaveChanges();
        }

        public void AcceptFriendRequest(string userId, string friendId)
        {
            var friendship = _context.Friendships.Where(f => f.User2Id == userId && f.User1Id == friendId).FirstOrDefault();

            if (friendship == null) return;
            if (friendship.Status != "Pending") return;

            friendship.Status = "Accepted";

            _context.Friendships.Update(friendship);
            _context.SaveChanges();
        }

        public void RemoveFriend(string userId, string friendId)
        {
            var friendship = _context.Friendships.Where(f => f.User2Id == userId && f.User1Id == friendId).FirstOrDefault();
            friendship ??= _context.Friendships.Where(f => f.User1Id == userId && f.User2Id == friendId).FirstOrDefault();
            if (friendship == null) return;

            _context.Friendships.Remove(friendship);
            _context.SaveChanges();
        }

        public List<FriendDTO> GetFriendsOf(string userId)
        {
            var friends = _context.Friendships.Where(f => f.User1Id == userId && f.Status == "Accepted").ToList();
            var friends2 = _context.Friendships.Where(f => f.User2Id == userId && f.Status == "Accepted").ToList();

            var friendDTOs = new List<FriendDTO>();

            foreach(var friend in friends)
            {
                FriendDTO.FromFriendship(userId, friend);
            }
            foreach(var friend in friends2)
            {
                FriendDTO.FromFriendship(userId, friend);
            }

            return friendDTOs;
        }
    }
}
