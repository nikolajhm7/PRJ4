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

        public async Task AddFriendRequest(string userId, string friendId)
        {
            if (userId == friendId) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);

            if (user == null || friend == null) return;

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

        public async Task AcceptFriendRequest(string userId, string friendId)
        {
            var friendship = await _context.Friendships.Where(f => f.User2Id == userId && f.User1Id == friendId)
                .FirstOrDefaultAsync();

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

        public async Task RemoveFriend(string userId, string friendId)
        {
            var friendship = await _context.Friendships.Where(f => f.User2Id == userId && f.User1Id == friendId)
                .FirstOrDefaultAsync();
            friendship ??= await _context.Friendships.Where(f => f.User1Id == userId && f.User2Id == friendId)
                .FirstOrDefaultAsync();
            if (friendship == null)
            {
                throw new Exception("Friendship not found");
            }

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FriendDTO>> GetFriendsOf(string userId, bool getInvites = false)
        {
            List<Friendship> friends;
            List<Friendship> friends2;
            if (!getInvites)
            {
                friends = await _context.Friendships.Where(f => f.User1Id == userId && f.Status == "Accepted")
                    .ToListAsync();
                friends2 = await _context.Friendships.Where(f => f.User2Id == userId && f.Status == "Accepted")
                    .ToListAsync();
            }
            else
            {
                friends = await _context.Friendships.Where(f => f.User1Id == userId).ToListAsync();
                friends2 = await _context.Friendships.Where(f => f.User2Id == userId).ToListAsync();
            }

            var friendDTOs = new List<FriendDTO>();

            foreach (var f in friends)
            {
                friendDTOs.Add(FriendDTO.FromFriendship(userId, f));
            }

            foreach (var f in friends2)
            {
                friendDTOs.Add(FriendDTO.FromFriendship(userId, f));
            }

            return friendDTOs;
        }
    }
}