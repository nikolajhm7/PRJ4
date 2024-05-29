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
[Route("[controller]")]
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
    [HttpGet("getAllGames")]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogDebug("Getting all games");
        var games = await _gameRepository.GetAllGames();
        if (games == null)
        {
            _logger.LogDebug("No games found");
            return NotFound();
        }

        return Ok(games);
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
            MaxPlayers = game.MaxPlayers
        };

        await _gameRepository.AddGame(newGame);
        
        _logger.LogDebug("Game {GameName} added.", newGame.Name);
        
        return Ok();
    }

    [HttpPut("editGame/{gameId}")]
    public async Task<IActionResult> EditGame([FromRoute] int gameId, [FromBody] GameDTO game)
    {
        _logger.LogDebug("Starting edit of game {GameName}.", game.Name);
        
        var existingGame = await _gameRepository.GetGameById(gameId);
        
        existingGame.Name = game.Name;
        existingGame.MaxPlayers = game.MaxPlayers;
        
        await _gameRepository.EditGame(existingGame);
        
        _logger.LogDebug("Game {GameName} edited.", existingGame.Name);
        
        return Ok();
    }

    [HttpDelete("DeleteGame")]
    public async Task<IActionResult> DeleteGame([FromBody] string gameName)
    {
        _logger.LogDebug("Starting deletion of game.");

        await _gameRepository.DeleteGame(gameName);

        _logger.LogDebug("Game {GameName} removed.", gameName);


        return Ok();
    }

    [HttpDelete("DeleteGameForUser")]
    public async Task<IActionResult> DeleteGameForUser([FromBody] GameUserDTO gameuser)
    {
        _logger.LogDebug("Starting deletion of game.");

        var game = await _gameRepository.GetGameById(gameuser.GameId);
        if (game == null)
        {
            return NotFound("game not found");
        }
        
        

        await _gameRepository.DeleteGameForUser(game.Name, gameuser.UserName);

        _logger.LogDebug("GameUser {GameName} and {username} removed.", game.Name,gameuser.UserName);


        return Ok();
    }

}