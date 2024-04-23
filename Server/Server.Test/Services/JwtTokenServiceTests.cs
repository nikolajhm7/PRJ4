using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Server.Test.Services;

public class JwtTokenServiceTests : TestBase
{

    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void GenerateToken_ShouldGenerateValidToken_ForToken()
    {
        var token = JwtTokenService.GenerateToken("User", false);
        
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
        ValidateJwtTokenStructure(token);
        
    }
    
    [Test]
    public void GenerateToken_ShouldGenerateValidToken_ForRefreshToken()
    {
        var token = JwtTokenService.GenerateRefreshToken("User");
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
        ValidateRefreshTokenStructure(token);
    }

    [Test]
    public void GenerateToken_ShouldIncludeGuestRole_IfIsGuestIsTrue()
    {
        var token = JwtTokenService.GenerateToken("Guest", true);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        Assert.That(roleClaim, Is.EqualTo("Guest"));
    }
    
    [Test]
    public void IsGuest_ShouldReturnTrue_IfRoleIsGuest()
    {
        var token = JwtTokenService.GenerateToken("GuestUser", true);
        var result = JwtTokenService.IsGuest(token);
    
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GetUserNameFromToken_ShouldReturnUserName()
    {
        var userName = "NormalUser";
        var token = JwtTokenService.GenerateToken(userName, false);
        var result = JwtTokenService.GetUserNameFromToken(token);
    
        Assert.That(result, Is.EqualTo(userName));
    }
    
    [Test]
    public void ValidateRefreshToken_ShouldReturnTrue_IfTokenIsValid()
    {
        var userName = "User";
        var refreshToken = JwtTokenService.GenerateRefreshToken(userName);
        TokenRepository.GetRefreshToken(userName).Returns(refreshToken);
        TokenRepository.IsActive(userName).Returns(true);

        var result = JwtTokenService.ValidateRefreshToken(userName, refreshToken);

        Assert.That(result, Is.True);
    }
    
    [Test]
    public void ValidateRefreshToken_ShouldReturnFalse_IfTokenIsInvalid()
    {
        var userName = "User";
        var refreshToken = JwtTokenService.GenerateRefreshToken(userName);
        TokenRepository.GetRefreshToken(userName).Returns(refreshToken);
        TokenRepository.IsActive(userName).Returns(false);

        var result = JwtTokenService.ValidateRefreshToken(userName, refreshToken);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GetTokenStringFromHttpContext_ShouldReturnTokenString_IfTokenIsInHeader()
    {
        var token = JwtTokenService.GenerateToken("User", false);
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Request.Headers["Authorization"] = "Bearer " + token;
        var result = JwtTokenService.GetTokenStringFromHttpContext(httpContext);

        Assert.That(result, Is.EqualTo(token));
    }
    
    [Test]
    public void IsGuest_WithGuestToken_ReturnsTrue()
    {
        // Arrange
        string token = JwtTokenService.GenerateToken("guest", isGuest: true); // Simuler et token for en gæst

        // Act
        bool result = JwtTokenService.IsGuest(token);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsGuest_WithUserToken_ReturnsFalse()
    {
        // Arrange
        string token = JwtTokenService.GenerateToken("user", isGuest: false); // Simuler et token for en almindelig bruger

        // Act
        bool result = JwtTokenService.IsGuest(token);

        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GetUserNameFromToken_ValidToken_ReturnsUserName()
    {
        // Arrange
        string expectedUserName = "testUser";
        string token = JwtTokenService.GenerateToken(expectedUserName);

        // Act
        string result = JwtTokenService.GetUserNameFromToken(token);

        // Assert
        Assert.That(result, Is.EqualTo(expectedUserName));
    }
    
    [Test]
    public void GenerateRefreshToken_GeneratesValidRefreshToken()
    {
        // Arrange
        string userName = "testUser";

        // Act
        string refreshToken = JwtTokenService.GenerateRefreshToken(userName);

        // Assert
        Assert.That(refreshToken, Is.Not.Null);
    }

    [Test]
    public void ValidateRefreshToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        string userName = "testUser";
        string refreshToken = "someRefreshToken";
        
        TokenRepository.GetRefreshToken(userName).Returns(refreshToken);
        TokenRepository.IsActive(userName).Returns(true);

        // Act
        bool isValid = JwtTokenService.ValidateRefreshToken(userName, refreshToken);

        // Assert
        Assert.That(isValid, Is.True);
    }
    
    [Test]
    public void ValidateUsername_ValidUsername_ReturnsTrue()
    {
        // Arrange
        string userName = "testUser";
        string token = JwtTokenService.GenerateToken(userName);

        // Act
        bool isValid = JwtTokenService.ValidateUsername(token, userName);

        // Assert
        Assert.That(isValid, Is.True);
    }
    
    [Test]
    public void GetTokenStringFromHttpContext_ValidHeader_ReturnsTokenString()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string expectedToken = "expectedToken";
        context.Request.Headers["Authorization"] = $"Bearer {expectedToken}";

        // Act
        string token = JwtTokenService.GetTokenStringFromHttpContext(context);

        // Assert
        Assert.That(token, Is.EqualTo(expectedToken));
    }

    [Test]
    public void IsTokenExpiring_TokenExpiringSoon_ReturnsTrue()
    {
        // Arrange
        string token = JwtTokenService.GenerateToken("testUser");
        
        TimeService.UtcNow.Returns(DateTime.UtcNow.AddMinutes(29));
        
        // Act
        bool isExpiring = JwtTokenService.IsTokenExpiring(token);

        // Assert
        Assert.That(isExpiring, Is.True);
    }
    

}
