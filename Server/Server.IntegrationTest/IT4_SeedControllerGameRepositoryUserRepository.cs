using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;

public class IT4_SeedControllerGameRepositoryUserRepository : IntegrationTestBase
{
    private SeedController _seedController;
    private UserManager<User> _userManager;
    private ILogger<SeedController> _logger;
    private IGameRepository _gameRepository;
    private IUserRepository _userRepository;

    [SetUp]
    public void SetUp()
    {
        
        var userStore = new UserStore<User>(Context);
        _userManager = new UserManager<User>(
            userStore,
            null, // Add options if necessary
            new PasswordHasher<User>(),
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new LoggerFactory().CreateLogger<UserManager<User>>()
        );

        _userRepository = new UserRepository(_userManager);
        _gameRepository = new GameRepository(Context, _userRepository);
        _logger = Substitute.For<ILogger<SeedController>>();

        _seedController = new SeedController(_logger, _gameRepository, _userRepository);
    }

    [Test]
    public async Task SeedData_AddsDataCorrectly()
    {
        // Arrange

        // Act
        var result = await _seedController.SeedData();

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());

        //ensure user with username Hugo123 exists
        var userExists = await Context.Users.AnyAsync(u => u.UserName == "Hugo123");
        var gameExists = await Context.Games.AnyAsync(g => g.Name == "hangman");

        Assert.That(userExists, Is.True);
        Assert.That(gameExists, Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }
}