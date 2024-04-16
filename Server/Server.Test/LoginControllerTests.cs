using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Services;

namespace Server.Test;

public class LoginControllerTests
{
    private UserManager<User>? _userManager;
    private ILogger<LoginController>? _logger;
    private IMemoryCache? _memoryCache;
    private LoginController? _controller;

    [SetUp]
    public void Setup()
    {
        // Mock afhængigheder
        _userManager = Substitute.For<UserManager<User>>(Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _logger = Substitute.For<ILogger<LoginController>>();
        _memoryCache = Substitute.For<IMemoryCache>();
        
        var configuration = Substitute.For<IConfiguration>();
        
        configuration["Jwt:Key"].Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes("yourVerySecretKeyHereThatIsLongEnoughToMeetTheRequirements")));
        configuration["Jwt:Issuer"].Returns("ExampleIssuer");
        configuration["Jwt:Audience"].Returns("ExampleAudience");
        
        var jwtTokenService = new JwtTokenService(configuration);
        
        _controller = new LoginController(_userManager, _logger, _memoryCache, jwtTokenService);
        
    }
    
    [Test]
    public async Task Login_Successful_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "testUser", Password = "correctPassword" };
        var user = new User { UserName = loginDto.UserName };
        _userManager.FindByNameAsync(loginDto.UserName).Returns(Task.FromResult(user));
        _userManager.CheckPasswordAsync(user, loginDto.Password).Returns(Task.FromResult(true));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        
        var token = objectResult.Value as string;
        Assert.That(objectResult, Is.Not.Null);
        var tokenParts = token.Split('.');
        Assert.That(tokenParts.Length, Is.EqualTo(3), "JWT should have 3 parts separated by '.'");
    }

    [Test]
    public async Task Login_WithIncorrectCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "nonExistingUser", Password = "wrongPassword" };
        _userManager.FindByNameAsync(loginDto.UserName).Returns(Task.FromResult<User>(null)); // Bruger findes ikke

        // Act
        var result = await _controller!.Login(loginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    

    
    [TearDown]
    public void TearDown()
    {
        _userManager?.Dispose();
        _memoryCache?.Dispose();
    }

}
