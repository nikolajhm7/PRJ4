using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NSubstitute;
using Server.API.Middleware;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using Server.API.Repository;

namespace Server.Test.Middleware;

public class TokenRefreshMiddlewareTests : TestBase
{
    private TokenRefreshMiddleware _middleware;
    private RequestDelegate _next;
    private DefaultHttpContext _httpContext;
    private ITokenRepository _tokenRepository;
    private IJwtTokenService _jwtTokenService;
    private IUserRepository _userRepository;

    [SetUp]
    public void SetUp()
    {
        _next = Substitute.For<RequestDelegate>();
        _tokenRepository = new TokenRepository(Context);
        _jwtTokenService = new JwtTokenService(Configuration, _tokenRepository, TimeService);
        _middleware = new TokenRefreshMiddleware(_next, _jwtTokenService);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        var userManager = new UserManager<User>(new UserStore<User>(Context), null, null, null, null, null, null, null, null);
        _userRepository = new UserRepository(userManager);
    }

    [Test]
    public async Task InvokeAsync_WithExpiringToken_ShouldRefreshToken()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken("testUser");
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        var refreshToken = _jwtTokenService.GenerateRefreshToken("testUser");
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
        
        await _userRepository.AddUser(newUser, "password");
        
        //Check whether the user is added to the database
        var user = await _userRepository.GetUserByName("testUser");
        Assert.That(user, Is.Not.Null);
        
        TimeService.UtcNow.Returns(DateTime.UtcNow.AddMinutes(29));  // 1 minut til udløb

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
        var token = _jwtTokenService.GenerateToken("testUser");
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";

        var refreshToken = _jwtTokenService.GenerateRefreshToken("testUser");
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

}