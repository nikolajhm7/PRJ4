using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Server.API.Data;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;

namespace Server.Test;


public class TestBase
{
    protected ApplicationDbContext? Context;
    protected IJwtTokenService? JwtTokenService;
    protected IConfiguration? Configuration;
    protected ITokenRepository? TokenRepository;
    protected ITimeService? TimeService;

    [SetUp]
    public virtual void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        Context = new ApplicationDbContext(options);
        
        Configuration = Substitute.For<IConfiguration>();
        TokenRepository = Substitute.For<ITokenRepository>();
        TimeService = Substitute.For<ITimeService>();
        TimeService.UtcNow.Returns(DateTime.Now);
        JwtTokenService = Substitute.For<JwtTokenService>(Configuration, TokenRepository, TimeService);

        // Setup konfigurationen
        Configuration["Jwt:Key"].Returns("verysecretkeyverysecretkeyverysecretkey");
        Configuration["Jwt:Issuer"].Returns("exampleIssuer");
        Configuration["Jwt:Audience"].Returns("exampleAudience");
    }
    
    protected void ValidateJwtTokenStructure(string token)
    {
        var accessTokenParts = token.Split('.');
        Assert.That(accessTokenParts.Length, Is.EqualTo(3), "JWT should have 3 parts separated by '.' for AccessToken");
    }
    
    protected void ValidateRefreshTokenStructure(string refreshToken)
    {
        Assert.That(refreshToken, Is.Not.Null.Or.Empty, "Refresh Token should not be null or empty");
        Assert.DoesNotThrow(() => Convert.FromBase64String(refreshToken), "Refresh Token should be a valid Base64 string");
        Assert.That(refreshToken.Length, Is.EqualTo(44), "Refresh Token should be 44 characters long");
    }

    [TearDown]
    public virtual void TearDown()
    {
        if (Context != null)
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
