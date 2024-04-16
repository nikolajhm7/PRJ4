using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repository;

namespace Server.Test.Repositories;

public class TokenRepositoryTests : TestBase
{

    [SetUp]
    public override void SetUp()
    {
        base.SetUp(); // This sets up the InMemory database

        // Nu kan du direkte arbejde med Context, som har en InMemory database
        Context.Users.AddRange(
            new User { Id = "1", RefreshTokens = new List<RefreshToken>() },
            new User { Id = "2", RefreshTokens = new List<RefreshToken>() }
        );
        Context.SaveChanges();

        TokenRepository = new TokenRepository(Context);
    }

    [Test]
    public void SaveRefreshToken_ShouldAddToken_IfUserExists()
    {
        string userId = "1";
        string refreshToken = "newToken123";
        DateTime expiryDate = DateTime.UtcNow.AddDays(7);

        TokenRepository.SaveRefreshToken(userId, refreshToken, expiryDate);

        User user = Context.Users.First(u => u.Id == userId);
        RefreshToken addedToken = user.RefreshTokens.Last();

        Assert.That(user.RefreshTokens, Has.Count.EqualTo(1));
        Assert.That(addedToken.Token, Is.EqualTo(refreshToken));
        Assert.That(addedToken.Expires, Is.EqualTo(expiryDate));
    }

    [Test]
    public void GetRefreshToken_ShouldReturnLatestToken()
    {
        string userId = "1";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "oldToken123", Created = DateTime.UtcNow.AddDays(-1) },
            new RefreshToken { Token = "newToken123", Created = DateTime.UtcNow }
        };
        Context.Users.First(u => u.Id == userId).RefreshTokens.AddRange(tokens);

        string result = TokenRepository.GetRefreshToken(userId);

        Assert.That(result, Is.EqualTo("newToken123"));
    }

    [Test]
    public void IsActive_ShouldReturnTrue_IfTokenHasNotExpired()
    {
        string userId = "1";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "olderToken123", Expires = DateTime.UtcNow.AddDays(-2) },
            new RefreshToken
                { Token = "newToken123", Expires = DateTime.UtcNow.AddMinutes(5) } // Token er stadig gyldig
        };
        Context.Users.First(u => u.Id == userId).RefreshTokens.AddRange(tokens);

        bool result = TokenRepository.IsActive(userId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsActive_ShouldReturnFalse_IfTokenHasExpired()
    {
        string userId = "1";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "olderToken123", Expires = DateTime.UtcNow.AddDays(-2) },
            new RefreshToken { Token = "newToken123", Expires = DateTime.UtcNow.AddMinutes(-5) } // Token er udløbet
        };
        Context.Users.First(u => u.Id == userId).RefreshTokens.AddRange(tokens);

        bool result = TokenRepository.IsActive(userId);

        Assert.That(result, Is.False);
    }
}