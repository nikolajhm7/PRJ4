using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Server.API.Repositories;
using Server.API.Repositories.Interfaces;

namespace Server.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FriendshipController> _logger;
        private readonly IFriendsRepository _friendsRepository;


        public FriendshipController(ApplicationDbContext context, IFriendsRepository friendsRepository, ILogger<FriendshipController> logger)
        {
            _context = context;
            _friendsRepository = friendsRepository;
            _logger = logger;
        }

        // PUT: api/Friendship/SendRequest
        [HttpPut("SendRequest")]
        public async Task<IActionResult> SendFriendRequest(string userId, string friendId)
        {
            _logger.LogDebug("Start sending friend request between {userId} and {friendId}",userId,friendId);

            try
            {
                // Check if users exist
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
                var friend =  await _context.Users.FirstOrDefaultAsync(u => u.UserName == friendId);

                if (user == null || friend == null)
                    return NotFound();

                // Check if friendship already exists
                var existingFriendship = await _context.Friendships
                    .Where(f => (f.User1Id == userId && f.User2Id == friendId) || (f.User1Id == friendId && f.User2Id == userId))
                    .FirstOrDefaultAsync();

                if (existingFriendship != null)
                    return BadRequest("Friend request already sent or friendship already exists.");

                // Create new friendship
                var newFriendship = new Friendship
                {
                    User1Id = user.Id,
                    User2Id = friend.Id,
                    Status = "Pending", // Set status to pending initially
                    date = DateTime.UtcNow
                };

                // Add and save changes
                _context.Friendships.Add(newFriendship);
                await _context.SaveChangesAsync();

                return Ok("Friend request sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Friendship/AcceptRequest
        [HttpPut("AcceptRequest")]
        public async Task<IActionResult> AcceptFriendRequest(string userId, string friendId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
                var friend = await _context.Users.FirstOrDefaultAsync(u => u.UserName == friendId);
                if (user == null || friend == null)
                    return NotFound();
                // Find the friendship
                var friendship = await _context.Friendships
                    .Where(f => ((f.User1Id == friend.Id && f.User2Id == user.Id) || ((f.User1Id == user.Id && f.User2Id == friend.Id))))
                    .FirstOrDefaultAsync();

                if (friendship == null)
                    return NotFound($"Friend request not found or already accepted/declined.{user.Id} and {friend.Id}");

                // Update friendship status to Accepted
                friendship.Status = "Accepted";

                // Save changes
                await _context.SaveChangesAsync();

                return Ok("Friend request accepted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteFriendship")]
        public async Task<IActionResult> DeleteFriendship([FromBody] FriendshipDTO friendshipDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == friendshipDto.Username);
                var friend = await _context.Users.FirstOrDefaultAsync(u => u.UserName == friendshipDto.FriendName);
                if (user == null || friend == null)
                    return NotFound();

                var friendship = await _context.Friendships
                    .Where(f => ((f.User1Id == friend.Id && f.User2Id == user.Id)||((f.User1Id == user.Id && f.User2Id == friend.Id))))
                    .FirstOrDefaultAsync();

                if (friendship == null)
                    return NotFound($"Friend request not found or already accepted/declined.{user.Id} and {friend.Id}");
                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
                return Ok("Friendship deleted succesfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }


        }
    }
}