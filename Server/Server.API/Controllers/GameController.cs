using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;

namespace Server.API.Controllers;

[ApiController]
[Route("games/[controller]")]
public class GameController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IJwtTokenService _jwtTokenService;
    
    public GameController(ApplicationDbContext context, ILogger<UserController> logger, IUserRepository userRepository, IGameRepository gameRepository, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _logger = logger;
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _jwtTokenService = jwtTokenService;
    }
    
    [Authorize]
    [HttpGet("getGamesForUser")]
    public async Task<IActionResult> GetGamesForUser([FromRoute] string userId)
    {
        _logger.LogDebug("Starting retrieval of games for user {UserId}.", userId);
    
        var user = await _userRepository.GetUserByName(userId);
    
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found.", userId);
            return NotFound();
        }

        var tokenString = _jwtTokenService.GetTokenString(HttpContext);
        
        if (_jwtTokenService.ValidateUsername(tokenString, userId))
        {
            _logger.LogWarning("User {TokenUsername} is trying to access games for another user ({UserId}).", _jwtTokenService.GetUserNameFromToken(tokenString), userId);
            return Unauthorized();
        }
        
        var games = await _gameRepository.GetGamesForUser(user);
        
        _logger.LogDebug("Retrieved {GameCount} games for user {UserId}.", games.Count, userId);
        
        return Ok(games);
    }
    
}