using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;

public class IT7_LoginControllerJwtTokenServiceUserRepository : IntegrationTestBase
{
    private LoginController _loginController;
    private IUserRepository _userRepository;
    private UserManager<User> _userManager;
    private ILogger<LoginController> _logger;
    
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
        _logger = Substitute.For<ILogger<LoginController>>();
        
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _loginController = new LoginController(_logger, memoryCache, JwtTokenService, _userRepository);
    }
    
    [Test]
    public async Task Login_ReturnsOkObjectResult_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            UserName = "Hugo123",
            Email = "hugo@hugo.dk",
        };
        
        await _userManager.CreateAsync(user, "Hugo1234");
        
        var loginDto = new LoginDto
        {
            UserName = "Hugo123",
            Password = "Hugo1234"
        };
        
        // Act
        var result = await _loginController.Login(loginDto);
        
        // Assert
        Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(200));
        
        Assert.That((result as ObjectResult)?.Value, Is.Not.Null);
        Assert.That((result as ObjectResult)?.Value, Has.Property("Token").Not.Null);
        Assert.That((result as ObjectResult)?.Value, Has.Property("RefreshToken").Not.Null);
    }
    
    [Test]
    public void LoginAsGuest_ReturnsOkObjectResult_WhenCalled()
    {
        // Arrange
        var guestLoginDto = new GuestLoginDTO
        {
            GuestName = "Guest123"
        };
        
        // Act
        var result = _loginController.LoginAsGuest(guestLoginDto);
        
        // Assert
        Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(200));
        
        Assert.That((result as ObjectResult)?.Value, Is.Not.Null);
        Assert.That((result as ObjectResult)?.Value, Has.Property("Token").Not.Null);
        Assert.That((result as ObjectResult)?.Value, Has.Property("RefreshToken").Not.Null);
    }
    
    
    
    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }
    
}