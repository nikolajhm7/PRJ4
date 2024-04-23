using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.Test;

public class LoginControllerTests : TestBase
{
    private ILogger<LoginController>? _logger;
    private IMemoryCache? _memoryCache;
    private LoginController? _controller;
    private IUserRepository? _userRepository;

    [SetUp]
    public void Setup()
    {
        
        _logger = Substitute.For<ILogger<LoginController>>();
        _memoryCache = Substitute.For<IMemoryCache>();
        _userRepository = Substitute.For<IUserRepository>();
        
        _controller = new LoginController(_logger, _memoryCache, JwtTokenService!, _userRepository);
        
    }
    
    [Test]
    public async Task Login_Successful_ReturnsTokens()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "testUser", Password = "correctPassword" };
        var user = new User { UserName = loginDto.UserName };
        _userRepository.GetUserByName(loginDto.UserName).Returns(Task.FromResult(user));
        _userRepository.UserCheckPassword(user, loginDto.Password).Returns(Task.FromResult(true));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        
        // Parse JSON to JsonElement
        var jsonElement = JsonDocument.Parse(JsonConvert.SerializeObject(objectResult.Value)).RootElement;
        var token = jsonElement.GetProperty("Token").GetString();
        var refreshToken = jsonElement.GetProperty("RefreshToken").GetString();

        ValidateJwtTokenStructure(token);
        ValidateRefreshTokenStructure(refreshToken);
        
    }
    
    [Test]
    public async Task Login_WithIncorrectCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "nonExistingUser", Password = "wrongPassword" };
        _userRepository.GetUserByName(loginDto.UserName).Returns(Task.FromResult<User?>(null));
        
        // Act
        var result = await _controller!.Login(loginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    
    [Test]
    public async Task LoginAsGuest_Suceessful_ReturnsTokens()
    {
        // Arrange
        var guestLoginDto = new GuestLoginDTO { GuestName = "GuestName" };
        var user = new User { UserName = guestLoginDto.GuestName };
        _userRepository.GetUserByName(guestLoginDto.GuestName).Returns(Task.FromResult<User?>(null));

        // Act
        var result = _controller!.LoginAsGuest(guestLoginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        
        // Parse JSON to JsonElement
        var jsonElement = JsonDocument.Parse(JsonConvert.SerializeObject(objectResult.Value)).RootElement;
        var token = jsonElement.GetProperty("Token").GetString();
        var refreshToken = jsonElement.GetProperty("RefreshToken").GetString();

        ValidateJwtTokenStructure(token);
        ValidateRefreshTokenStructure(refreshToken);
    }
    
    [Test]
    public async Task Login_TooManyAttempts_ReturnsTooManyRequests()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "lockedUser", Password = "anyPassword" };
        var cacheKey = $"login_attempts_for_{loginDto.UserName}";
        _memoryCache.TryGetValue(cacheKey, out int _).Returns(x =>
        {
            x[1] = 5;  // Simulate 5 failed attempts already in cache
            return true;
        });

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status429TooManyRequests));
    }
    
    [Test]
    public void LoginAsGuest_InvalidGuestName_ReturnsBadRequest()
    {
        // Arrange
        var guestLoginDto = new GuestLoginDTO { GuestName = "Invalid Name!" };

        // Act
        var result = _controller.LoginAsGuest(guestLoginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public void LoginAsGuest_EmptyGuestName_ReturnsBadRequest()
    {
        // Arrange
        var guestLoginDto = new GuestLoginDTO { GuestName = "" };

        // Act
        var result = _controller.LoginAsGuest(guestLoginDto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "testUser", Password = "wrongPassword" };
        var user = new User { UserName = loginDto.UserName };
        _userRepository.GetUserByName(loginDto.UserName).Returns(Task.FromResult(user));
        _userRepository.UserCheckPassword(user, loginDto.Password).Returns(Task.FromResult(false));

        // Act
        var result = await _controller!.Login(loginDto);

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    
    [Test]
    public async Task Login_CacheKeyIsSetAttemptsNotExceeded()
    {
        // Arrange
        var loginDto = new LoginDto { UserName = "lockedUser", Password = "anyPassword" };
        var cacheKey = $"login_attempts_for_{loginDto.UserName}";
        _memoryCache.TryGetValue(cacheKey, out int _).Returns(x =>
        {
            x[1] = 1;  // Simulate 1 failed attempt already in cache
            return true;
        });

        // Act
        var result = await _controller.Login(loginDto);
        
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.Not.EqualTo(StatusCodes.Status429TooManyRequests));
    }
    
    [TearDown]
    public void TearDown()
    {
        _memoryCache?.Dispose();
    }

}
