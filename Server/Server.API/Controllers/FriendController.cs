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
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FriendshipController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUT: api/Friendship/SendRequest
        [HttpPut("SendRequest")]
        public async Task<IActionResult> SendFriendRequest(string userId, string friendId)
        {
            try
            {
                // Check if users exist
                var user = await _context.Users.FindAsync(userId);
                var friend = await _context.Users.FindAsync(friendId);

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
                    User1Id = userId,
                    User2Id = friendId,
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
                // Find the friendship
                var friendship = await _context.Friendships
                    .Where(f => (f.User1Id == friendId && f.User2Id == userId && f.Status == "Pending"))
                    .FirstOrDefaultAsync();

                if (friendship == null)
                    return NotFound("Friend request not found or already accepted/declined.");

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
    }
}