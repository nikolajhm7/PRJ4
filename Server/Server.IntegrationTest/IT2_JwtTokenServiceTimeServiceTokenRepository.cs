using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;

public class IT2_JwtTokenServiceTimeServiceTokenRepository : IntegrationTestBase
{
    private IJwtTokenService _jwtTokenService;
    private ITimeService _timeService;
    private ITokenRepository _tokenRepository;
    private IConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        base.SetUp();
        
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "very_long_and_secure_key_that_should_be_long_enough"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();


        _timeService = new TimeService();
        _tokenRepository = new TokenRepository(Context);
        _jwtTokenService = new JwtTokenService(_configuration, _tokenRepository, _timeService);

    }
    
    [Test]
    public void Token_Generation_And_Validation()
    {
        // Arrange
        var userName = "testuser";

        // Act
        var token = _jwtTokenService.GenerateToken(userName, false);
        var usernameFromToken = _jwtTokenService.GetUserNameFromToken(token);
        var isGuest = _jwtTokenService.IsGuest(token);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(usernameFromToken, Is.EqualTo(userName));
        Assert.That(isGuest, Is.False);
        
    }

    [Test]
    public void Refresh_Token_Validation()
    {
        // Arrange
        var userName = "testuser";
        Context.Users.Add(new User { UserName = userName });
        Context.SaveChanges();
        
        var token = _jwtTokenService.GenerateRefreshToken(userName);
        
        // Act
        var isValid = _jwtTokenService.ValidateRefreshToken(userName, token);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Token_Expiry_Check()
    {
        // Arrange
        var userName = "testuser";
        var token = _jwtTokenService.GenerateToken(userName);

        // Wait to simulate time passing (use Thread.Sleep in a real test or adjust the TimeService to simulate time)
        _timeService.AdvanceTime(20);

        // Act
        var isExpiring = _jwtTokenService.IsTokenExpiring(token);

        // Assert
        Assert.That(isExpiring, Is.True);
    }
    
}