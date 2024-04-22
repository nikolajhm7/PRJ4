using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;

namespace Server.API.Controllers;

[ApiController]
[Route("games/[controller]")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IJwtTokenService _jwtTokenService;
    
    public GameController(ILogger<GameController> logger, IUserRepository userRepository, IGameRepository gameRepository, IJwtTokenService jwtTokenService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _jwtTokenService = jwtTokenService;
    }
    
    [Authorize]
    [HttpGet("getGamesForUser/{userId}")]
    public async Task<IActionResult> GetGamesForUser([FromRoute] string userId)
    {
        _logger.LogDebug("Starting retrieval of games for user {UserId}.", userId);
    
        var user = await _userRepository.GetUserByName(userId);
    
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found.", userId);
            return NotFound();
        }

        var tokenString = _jwtTokenService.GetTokenStringFromHttpContext(HttpContext);
        
        if (!_jwtTokenService.ValidateUsername(tokenString, userId))
        {
            _logger.LogWarning("User {TokenUsername} is trying to access games for another user ({UserId}).", _jwtTokenService.GetUserNameFromToken(tokenString), userId);
            return Unauthorized();
        }
        
        var games = await _gameRepository.GetGamesForUser(user);
        
        _logger.LogDebug("Retrieved {GameCount} games for user {UserId}.", games.Count, userId);
        
        return Ok(games);
    }
    
    [HttpPost("addGameForUser")]
    public async Task<IActionResult> AddGameForUser([FromBody] GameUserDTO gameUserDto)
    {
        _logger.LogDebug("Starting addition of game {GameId} for user {UserId}.", gameUserDto.GameId, gameUserDto.UserName);
    
        var user = await _userRepository.GetUserByName(gameUserDto.UserName);
    
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found.", gameUserDto.UserName);
            return NotFound();
        }
        
        await _gameRepository.AddGameToUser(gameUserDto.UserName, gameUserDto.GameId);
        
        _logger.LogDebug("Game {GameId} added to user {UserId}.", gameUserDto.GameId, gameUserDto.UserName);
        
        return Ok();
    }
    
    [HttpPost("addGame")]
    public async Task<IActionResult> AddGame([FromBody] GameDTO game)
    {
        _logger.LogDebug("Starting addition of game {GameName}.", game.Name);
    
        var newGame = new Game
        {
            Name = game.Name,
        };

        await _gameRepository.AddGame(newGame);
        
        _logger.LogDebug("Game {GameName} added.", newGame.Name);
        
        return Ok();
    }
}