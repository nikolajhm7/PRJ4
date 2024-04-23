using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;

namespace Server.Test.Repositories;

public class GameRepositoryTests : TestBase
{
    private GameRepository _gameRepository;
    private IUserRepository _userRepository;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        
        _userRepository = Substitute.For<IUserRepository>();

        _gameRepository = new GameRepository(Context, _userRepository);
        
    }
    
    [Test]
    public async Task GetGamesForUser_ReturnsCorrectGames()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var game1 = new Game { GameId = 1, Name = "Game1" };
        var game2 = new Game { GameId = 2, Name = "Game2" };
        Context.Users.Add(user);
        Context.Games.AddRange(game1, game2);
        Context.UserGames.Add(new UserGame { UserId = user.Id, GameId = game1.GameId });
        Context.SaveChanges();

        // Act
        var games = await _gameRepository.GetGamesForUser(user);

        // Assert
        Assert.That(games, Is.Not.Null);
        Assert.That(games.Count, Is.EqualTo(1));
        Assert.That(games[0].GameId, Is.EqualTo(game1.GameId));
    }
    
    [Test]
    public async Task AddGameToUser_AddsGameSuccessfully()
    {
        // Arrange
        var user = new User { Id = "user1", UserName = "username" };
        var game = new Game { GameId = 1, Name = "Game1" };
        _userRepository.GetUserByName("username").Returns(Task.FromResult(user));
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act
        await _gameRepository.AddGameToUser(user.UserName, game.GameId);

        // Assert
        var userGames = await Context.UserGames.ToListAsync();
        Assert.That(userGames, Is.Not.Null);
        Assert.That(userGames.Count, Is.EqualTo(1));
        Assert.That(userGames[0].UserId, Is.EqualTo(user.Id));
        Assert.That(userGames[0].GameId, Is.EqualTo(game.GameId));
    }
    
    [Test]
    public async Task AddGameToUser_ThrowsExceptionIfUserNotFound()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.AddGameToUser("username", game.GameId));
    }
    
    [Test]
    public async Task AddGameToUser_ThrowsExceptionIfGameNotFound()
    {
        // Arrange
        var user = new User { Id = "user1", UserName = "username" };
        _userRepository.GetUserByName("username").Returns(Task.FromResult(user));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.AddGameToUser(user.UserName, 1));
    }

    [Test]
    public async Task AddGame_ThrowsExceptionIfGameExists()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act & Assert
        var newGame = new Game { GameId = 1, Name = "Game1" };
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.AddGame(newGame));
    }
    
    [Test]
    public async Task AddGame_AddsGameSuccessfully()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };
        
        // Act
        await _gameRepository.AddGame(game);

        // Assert
        var games = await Context.Games.ToListAsync();
        Assert.That(games, Is.Not.Null);
        Assert.That(games.Count, Is.EqualTo(1));
        Assert.That(games[0].GameId, Is.EqualTo(game.GameId));
    }


}
