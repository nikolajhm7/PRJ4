using Server.API.Data;
using Server.API.Repositories.Interfaces;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Server.API.Repositories
{
    public class FriendsRepository : IFriendsRepository
    {
        private readonly ApplicationDbContext _context;

        public FriendsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        private async Task<User?> FindUserFromUserName(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task AddFriendRequest(string userName, string friendName)
        {
            if (userName == friendName)
            {
                throw new Exception("Cannot add yourself as a friend");
            }
            

            var user = await FindUserFromUserName(userName);
            var friend = await FindUserFromUserName(friendName);

            if (user == null)
            {
                throw new Exception("User not found");
            }
            
            if (friend == null)
            {
                throw new Exception("Friend not found");
            }

            var friendRequest = new Friendship
            {
                User1Id = user.Id,
                User1 = user,
                User2Id = friend.Id,
                User2 = friend,
                Status = "Pending",
                date = DateTime.Now
            };

            await _context.Friendships.AddAsync(friendRequest);
            await _context.SaveChangesAsync();
        }
        
        private async Task<Friendship> FindFriendshipOneWay(string user1UserName, string user2UserName)
        {
            return await _context.Friendships.Where(f => f.User1.UserName == user1UserName && f.User2.UserName == user2UserName)
                .FirstOrDefaultAsync();
        }
        
        private async Task<Friendship> FindFriendship(string userName, string friendName)
        {
            var friendship = await FindFriendshipOneWay(userName, friendName);
            if (friendship == null)
            {
                friendship = await FindFriendshipOneWay(friendName, userName);
            }

            return friendship;
        }

        public async Task AcceptFriendRequest(string userName, string friendName)
        {
            var friendship = await FindFriendshipOneWay(friendName, userName);

            if (friendship == null)
            {
                throw new Exception("Friendship not found");
            }

            if (friendship.Status != "Pending")
            {
                throw new Exception("Friendship not pending");
            }

            friendship.Status = "Accepted";

            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriend(string userName, string friendName)
        {
            var friendship = await FindFriendship(userName, friendName);
            
            if (friendship == null)
            {
                throw new Exception("Friendship not found");
            }

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FriendDTO>> GetInvitesOf(string userName)
        {
            List<Friendship> friends;
            List<Friendship> friends2;
            
            friends = await _context.Friendships.Where(f => f.User1.UserName == userName).ToListAsync();
            friends2 = await _context.Friendships.Where(f => f.User2.UserName == userName).ToListAsync();
            
            var friendDTOs = new List<FriendDTO>();
            
            foreach (var f in friends)
            {
                friendDTOs.Add(FriendDTO.FormFriendship(userName, f));
            }
            
            foreach (var f in friends2)
            {
                friendDTOs.Add(FriendDTO.FormFriendship(userName, f));
            }
            
            return friendDTOs;
        }
        
        
        public async Task<List<FriendDTO>> GetFriendsOf(string userName)
        {
            List<Friendship> friends;
            List<Friendship> friends2;
            
            friends = await _context.Friendships
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.User1.UserName == userName && f.Status == "Accepted")
                .ToListAsync();

            friends2 = await _context.Friendships
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.User2.UserName == userName && f.Status == "Accepted")
                .ToListAsync();

            var friendDTOs = new List<FriendDTO>();

            foreach (var f in friends)
            {
                friendDTOs.Add(FriendDTO.FormFriendship(userName, f));
            }
            
            foreach (var f in friends2)
            {
                friendDTOs.Add(FriendDTO.FormFriendship(userName, f));
            }

            return friendDTOs;
        }
    }
}