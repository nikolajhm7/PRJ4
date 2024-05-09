using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Server.API.Middleware;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using Server.API.Repository;

namespace Server.Test.Middleware;

public class TokenRefreshMiddlewareTests : IntegrationTestBase
{
    private TokenRefreshMiddleware _middleware;
    private RequestDelegate _next;
    private DefaultHttpContext _httpContext;
    private IUserRepository _userRepository;

    [SetUp]
    public void SetUp()
    {
        _next = Substitute.For<RequestDelegate>();
        _middleware = new TokenRefreshMiddleware(_next, JwtTokenService);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        var userStore = Substitute.For<IUserStore<User>>();

        var userManager = Substitute.For<UserManager<User>>(
            userStore, 
            Substitute.For<IOptions<IdentityOptions>>(), 
            Substitute.For<IPasswordHasher<User>>(),
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<User>>>());
        
        _userRepository = new UserRepository(userManager);
    }

    [Test]
    public async Task InvokeAsync_WithExpiringToken_ShouldRefreshToken()
    {
        // Arrange
        var token = JwtTokenService.GenerateToken("testUser");
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        var refreshToken = JwtTokenService.GenerateRefreshToken("testUser");
        
        _httpContext.Request.Headers["X-Refresh-Token"] = refreshToken;

        var newUser = new User
        {
            Id = "testUser",
            UserName = "testUser",
            PasswordHash = "password",
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Created = DateTime.UtcNow
                }
            }
        };
        
        var addedUser = await _userRepository.AddUser(newUser, "password");
        
        TimeService.UtcNow.Returns(DateTime.UtcNow.AddMinutes(29));  // 1 minut til udløb
        
        TokenRepository.GetRefreshToken("testUser").Returns(refreshToken);
        TokenRepository.IsActive("testUser").Returns(true);
        
        await _middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.That(_httpContext.Response.Headers["X-New-AccessToken"], Is.Not.Empty);
        Assert.That(_httpContext.Response.Headers["X-New-RefreshToken"], Is.Not.Empty);
        
        Assert.That(_httpContext.Response.Headers["X-New-AccessToken"], Is.Not.EqualTo(token));
        Assert.That(_httpContext.Response.Headers["X-New-RefreshToken"], Is.Not.EqualTo(refreshToken));

        ValidateJwtTokenStructure(_httpContext.Response.Headers["X-New-AccessToken"]);
        ValidateRefreshTokenStructure(_httpContext.Response.Headers["X-New-RefreshToken"]);
        
        await _next.Received(1).Invoke(_httpContext);
        
    }

    [Test]
    public async Task InvokeAsync_WithNotExpiringToken_ShouldNotRefreshToken()
    {
        TimeService.UtcNow.Returns(DateTime.UtcNow);
        // Arrange
        var token = JwtTokenService.GenerateToken("testUser");
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";

        var refreshToken = JwtTokenService.GenerateRefreshToken("testUser");
        _httpContext.Request.Headers["X-Refresh-Token"] = refreshToken;

        Context.Users.Add(
            new User
            {
                Id = "testUser",
                UserName = "testUser",
                PasswordHash = "password",
                RefreshTokens = new List<RefreshToken>
                {
                    new RefreshToken
                    {
                        Token = refreshToken,
                        Expires = DateTime.UtcNow.AddMinutes(30),
                        Created = DateTime.UtcNow
                    }
                }
            }
        );
        Context.SaveChanges();

        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Response.Headers["X-New-AccessToken"], Is.Empty);
        Assert.That(_httpContext.Response.Headers["X-New-RefreshToken"], Is.Empty);

        await _next.Received(1).Invoke(_httpContext);
    }

    
    private void ValidateJwtTokenStructure(string token)
    {
        var accessTokenParts = token.Split('.');
        Assert.That(accessTokenParts.Length, Is.EqualTo(3), "JWT should have 3 parts separated by '.' for AccessToken");
    }
    
    private void ValidateRefreshTokenStructure(string refreshToken)
    {
        Assert.That(refreshToken, Is.Not.Null.Or.Empty, "Refresh Token should not be null or empty");
        Assert.DoesNotThrow(() => Convert.FromBase64String(refreshToken), "Refresh Token should be a valid Base64 string");
        Assert.That(refreshToken.Length, Is.EqualTo(44), "Refresh Token should be 44 characters long");
    }
}