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
    
    [Test]
    public async Task GetMaxPlayers_ReturnsCorrectMaxPlayers()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1", MaxPlayers = 5 };
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act
        var maxPlayers = await _gameRepository.GetMaxPlayers(game.GameId);

        // Assert
        Assert.That(maxPlayers, Is.EqualTo(game.MaxPlayers));
    }
    
    [Test]
    public async Task GetMaxPlayers_ThrowsExceptionIfGameNotFound()
    {
        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.GetMaxPlayers(1));
    }
    
    [Test]
    public async Task GetGameById_ReturnsCorrectGame()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act
        var result = await _gameRepository.GetGameById(game.GameId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.GameId, Is.EqualTo(game.GameId));
    }
    
    [Test]
    public async Task GetGameById_ThrowsExceptionIfGameNotFound()
    {
        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.GetGameById(1));
    }
    
    [Test]
    public async Task EditGame_ThrowsExceptionIfGameNotFound()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await _gameRepository.EditGame(game));
    }
    
    [Test]
    public async Task EditGame_EditsGameSuccessfully()
    {
        // Arrange
        var game = new Game { GameId = 1, Name = "Game1" };
        Context.Games.Add(game);
        Context.SaveChanges();

        // Act
        game.Name = "Game2";
        game.MaxPlayers = 5;
        await _gameRepository.EditGame(game);

        // Assert
        var editedGame = await Context.Games.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        Assert.That(editedGame, Is.Not.Null);
        Assert.That(editedGame.Name, Is.EqualTo(game.Name));
        Assert.That(editedGame.MaxPlayers, Is.EqualTo(game.MaxPlayers));
    }

    [Test]
    public async Task GetAllGames_ReturnsAllGames()
    {
        // Arrange
        var game1 = new Game { GameId = 1, Name = "Game1" };
        var game2 = new Game { GameId = 2, Name = "Game2" };
        var game3 = new Game { GameId = 3, Name = "Game3" };

        Context.Games.AddRange(game1, game2, game3);
        Context.SaveChanges();

        // Act
        var games = await _gameRepository.GetAllGames();

        // Assert
        Assert.That(games, Is.Not.Null);
        Assert.That(games.Count, Is.EqualTo(3));
        Assert.That(games.Any(g => g.GameId == game1.GameId && g.Name == game1.Name), Is.True);
        Assert.That(games.Any(g => g.GameId == game2.GameId && g.Name == game2.Name), Is.True);
        Assert.That(games.Any(g => g.GameId == game3.GameId && g.Name == game3.Name), Is.True);
    }

}
