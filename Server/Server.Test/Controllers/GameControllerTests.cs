using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;

namespace Server.Test;

public class GameControllerTests : TestBase
{
    private GameController _controller;
    private IUserRepository _userRepository;
    private IGameRepository _gameRepository;
    private IJwtTokenService _jwtTokenService;
    private ILogger<GameController> _logger;
    private HttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _gameRepository = Substitute.For<IGameRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _logger = Substitute.For<ILogger<GameController>>();
        _httpContext = new DefaultHttpContext();
        
        var controllerContext = new ControllerContext()
        {
            HttpContext = _httpContext
        };

        _controller = new GameController(_logger, _userRepository, _gameRepository, _jwtTokenService)
        {
            ControllerContext = controllerContext
        };
        
    }
    
    [Test]
    public async Task GetGamesForUser_ReturnsNotFound_WhenUserNotFound()
    {
        string userId = "user1";
        _userRepository.GetUserByName(userId).Returns(Task.FromResult<User>(null));

        var result = await _controller.GetGamesForUser(userId) as NotFoundResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetGamesForUser_ReturnsUnauthorized_WhenTokenValidationFails()
    {
        string userId = "user1";
        User user = new User { UserName = userId };
        _userRepository.GetUserByName(userId).Returns(user);
        _jwtTokenService.GetTokenStringFromHttpContext(Arg.Any<HttpContext>()).Returns("token");
        _jwtTokenService.ValidateUsername("token", userId).Returns(false);

        var result = await _controller.GetGamesForUser(userId) as UnauthorizedResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetGamesForUser_ReturnsOk_WhenGamesAreFound()
    {
        string userId = "user1";
        User user = new User { UserName = userId };
        List<Game> games = new List<Game> { new Game { Name = "Game1" } };
        _userRepository.GetUserByName(userId).Returns(user);
        _jwtTokenService.GetTokenStringFromHttpContext(Arg.Any<HttpContext>()).Returns("token");
        _jwtTokenService.ValidateUsername("token", userId).Returns(true);
        _gameRepository.GetGamesForUser(user).Returns(games);

        var result = await _controller.GetGamesForUser(userId) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(games));
    }
    
    [Test]
    public async Task AddGameForUser_ReturnsNotFound_WhenUserNotFound()
    {
        string userName = "user1";
        GameUserDTO gameUserDto = new GameUserDTO { UserName = userName };
        _userRepository.GetUserByName(userName).Returns(Task.FromResult<User>(null));

        var result = await _controller.AddGameForUser(gameUserDto) as NotFoundResult;

        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task AddGameForUser_ReturnsOk_WhenGameIsAdded()
    {
        string userName = "user1";
        GameUserDTO gameUserDto = new GameUserDTO { UserName = userName, GameId = 1 };
        User user = new User { UserName = userName };
        _userRepository.GetUserByName(userName).Returns(user);

        var result = await _controller.AddGameForUser(gameUserDto) as OkResult;

        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task AddGame_ReturnsOk_WhenGameIsAdded()
    {
        GameDTO game = new GameDTO { Name = "Game1" };

        var result = await _controller.AddGame(game) as OkResult;

        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task EditGame_ReturnsOk_WhenGameIsEdited()
    {
        GameDTO game = new GameDTO { Name = "Game1" };
        int gameId = 1;
        
        _gameRepository.GetGameById(gameId).Returns(new Game());

        var result = await _controller.EditGame(gameId, game) as OkResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAllGames_ReturnsOk_WhenGamesAreFound()
    {
        // Arrange
        var games = new List<Game> { new Game { Name = "Game1" }, new Game { Name = "Game2" } };
        _gameRepository.GetAllGames().Returns(Task.FromResult(games));

        // Act
        var result = await _controller.GetAllGames() as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(games));
    }

    [Test]
    public async Task GetAllGames_ReturnsNotFound_WhenNoGamesAreFound()
    {
        // Arrange
        List<Game> games = null;
        _gameRepository.GetAllGames().Returns(Task.FromResult(games));

        // Act
        var result = await _controller.GetAllGames() as NotFoundResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

}
