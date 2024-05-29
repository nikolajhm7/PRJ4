using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.API.Controllers;

public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public UserController(ILogger<UserController> logger, IUserRepository userRepository, ApplicationDbContext context)
    {
        _logger = logger;
        _userRepository = userRepository;
        _context = context;
    }

    [HttpGet("GetUserIdFromUsername/{username}")]
    public async Task<IActionResult> GetUserIdFromUsername(string username)
    {
        var user = await _context.Users
            .Where(u => u.UserName == username)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user.Id);
    }


    [HttpPost("makeNewUser")]
    public async Task<IActionResult> MakeNewUser([FromBody] RegisterDto registerDto)
    {
        _logger.LogDebug("Starting creation of new user.");
    
        if (!ModelState.IsValid)
        {
            _logger.LogDebug("Model validation failed: {ModelState}", ModelState);
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors = errors });
        }

        var newUser = new User
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email
        };

        _logger.LogDebug("Attempting to create user: {UserName}", newUser.UserName);
    
        var result = await _userRepository.AddUser(newUser, registerDto.Password);

        if (!result.Succeeded)
        {
            _logger.LogDebug("User creation failed: {Errors}", result.Errors);
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { errors = errors });
        }

        _logger.LogDebug("User {UserName} created successfully.", newUser.UserName);
    
        return Ok(new { message = "User created successfully." });
    }

    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUser([FromBody] string username)
    {
        _logger.LogDebug("Starting deletion of user.");
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogDebug("Model validation failed: {ModelState}", ModelState);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors = errors });
            }

            var user = await _context.Users
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("User not found");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok("User removed successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}