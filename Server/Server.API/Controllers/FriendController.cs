using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FriendshipController> _logger;

        public FriendshipController(ApplicationDbContext context, ILogger<FriendshipController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // PUT: api/Friendship/SendRequest
        [HttpPut("SendRequest")]
        public async Task<IActionResult> SendFriendRequest(string userId, string friendId)
        {
            _logger.LogInformation("Attempting to send friend request from user {UserId} to {FriendId}", userId, friendId);

            var user = await _context.Users.FindAsync(userId);
            var friend = await _context.Users.FindAsync(friendId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return NotFound("User not found.");
            }
            
            if (friend == null)
            {
                _logger.LogWarning("Friend with ID {FriendId} not found.", friendId);
                return NotFound("Friend not found.");
            }

            var existingFriendship = await _context.Friendships
                .Where(f => (f.User1Id == userId && f.User2Id == friendId) ||
                            (f.User1Id == friendId && f.User2Id == userId))
                .FirstOrDefaultAsync();

            if (existingFriendship != null)
            {
                _logger.LogWarning("Friend request between {UserId} and {FriendId} already exists.", userId, friendId);
                return BadRequest("Friend request already sent or friendship already exists.");
            }

            var newFriendship = new Friendship
            {
                User1Id = userId,
                User2Id = friendId,
                Status = "Pending",
                date = DateTime.UtcNow
            };

            _context.Friendships.Add(newFriendship);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Friend request from {UserId} to {FriendId} sent successfully.", userId, friendId);
            return Ok("Friend request sent successfully.");
        }

        // PUT: api/Friendship/AcceptRequest
        [HttpPut("AcceptRequest")]
        public async Task<IActionResult> AcceptFriendRequest(string userId, string friendId)
        {
            _logger.LogInformation("Attempting to accept friend request for {UserId} from {FriendId}", userId, friendId);

            var friendship = await _context.Friendships
                .Where(f => (f.User1Id == friendId && f.User2Id == userId && f.Status == "Pending"))
                .FirstOrDefaultAsync();

            if (friendship == null)
            {
                _logger.LogWarning("Friend request between {FriendId} and {UserId} not found or already processed.", friendId, userId);
                return NotFound("Friend request not found or already accepted/declined.");
            }

            friendship.Status = "Accepted";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Friend request between {FriendId} and {UserId} accepted successfully.", friendId, userId);
            return Ok("Friend request accepted successfully.");
        }
    }
}