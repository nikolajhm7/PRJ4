using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.API.DTO;
using Server.API.Data;
using Server.API.Models;

namespace Server.API.Controllers;

public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserController> _logger;
    
    public UserController(ApplicationDbContext context, UserManager<User> userManager, ILogger<UserController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
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
    
        var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded)
        {
            _logger.LogDebug("User creation failed: {Errors}", result.Errors);
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { errors = errors });
        }

        _logger.LogDebug("User {UserName} created successfully.", newUser.UserName);
    
        return Ok(new { message = "User created successfully." });
    }
    
}