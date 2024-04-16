using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    
}
