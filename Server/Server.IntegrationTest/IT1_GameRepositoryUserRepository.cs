using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;


public class IT1_GameRepositoryUserRepository : IntegrationTestBase
{
    private IGameRepository _gameRepository;
    private IUserRepository _userRepository;
    private UserManager<User> _userManager;

    [SetUp]
    public void SetUp()
    {
        base.SetUp();
        
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _userRepository = new UserRepository(_userManager);
        _gameRepository = new GameRepository(Context, _userRepository);
    }
    
    [Test]
    public async Task AddGameToUser_UserDoesNotExist_ThrowsException()
    {
        //add game with name
        var game = new Game { GameId = 1, Name = "Test Game" };
            
        await Context.Games.AddAsync(game);
        await Context.SaveChangesAsync();
        
        Assert.ThrowsAsync<Exception>(() => _gameRepository.AddGameToUser("UserDoesNotExist", game.GameId));
    }
    
    [Test]
    public async Task AddGameToUser_AddsGame_WhenUserAndGameExist()
    {
        // Arrange
        var user = new User { UserName = "testuser" };
        var game = new Game { GameId = 1, Name = "Test Game" };
        await Context.Users.AddAsync(user);
        await Context.Games.AddAsync(game);
        await Context.SaveChangesAsync();
        
        _userManager.FindByNameAsync(user.UserName).Returns(user);

        // Act
        await _gameRepository.AddGameToUser(user.UserName, game.GameId);

        // Assert
        var userGames = await Context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == user.Id && ug.GameId == game.GameId);
        Assert.That(userGames, Is.Not.Null);
    }
    
    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }
    
}