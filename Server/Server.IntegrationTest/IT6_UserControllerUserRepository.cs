using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.Data;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;

public class IT6_UserControllerUserRepository : IntegrationTestBase
{
    private UserController _userController;
    private IUserRepository _userRepository;
    private ILogger<UserController> _logger;
    private UserManager<User> _userManager;
    private ApplicationDbContext _context;
    
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
        _logger = Substitute.For<ILogger<UserController>>();
        
        _userController = new UserController(_logger, _userRepository, _context);
    }

    [Test]
    public async Task MakeNewUser_CreatesUserCorrectly()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "Hugo123",
            Email = "Hugo@Hugo.dk",
            Password = "Hugo1234"
        };

        // Act
        var result = await _userController.MakeNewUser(registerDto);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        Assert.That(Context.Users.Any(u => u.UserName == "Hugo123"), Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }
}