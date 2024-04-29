using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository;

namespace Server.Test.Repositories;

public class TokenRepositoryTests : TestBase
{

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
    }

    [Test]
    public void SaveRefreshToken_SavesTokenToDatabase()
    {
        // Arrange
        var repository = new TokenRepository(Context);
        string userId = "testUser";
        string refreshToken = "testToken";
        DateTime expiryDate = DateTime.UtcNow.AddDays(7);

        // Forbered en bruger i databasen
        Context.Users.Add(new User { Id = userId, RefreshTokens = new List<RefreshToken>() });
        Context.SaveChanges();

        // Act
        repository.SaveRefreshToken(userId, refreshToken, expiryDate);

        // Assert
        var user = Context.Users.Include(u => u.RefreshTokens).FirstOrDefault(u => u.Id == userId);
        Assert.That(user.RefreshTokens, Is.Not.Null);
        Assert.That(user.RefreshTokens, Is.Not.Empty);
        Assert.That(user.RefreshTokens.Last().Token, Is.EqualTo(refreshToken));
        Assert.That(user.RefreshTokens.Last().Expires, Is.EqualTo(expiryDate));
    }
    
    [Test]
    public void GetRefreshToken_ReturnsLatestToken()
    {
        // Arrange
        var repository = new TokenRepository(Context);
        string userId = "testUser";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "oldToken", Created = DateTime.UtcNow.AddDays(-10) },
            new RefreshToken { Token = "newToken", Created = DateTime.UtcNow }
        };

        Context.Users.Add(new User { Id = userId, RefreshTokens = tokens });
        Context.SaveChanges();

        // Act
        var result = repository.GetRefreshToken(userId);

        // Assert
        Assert.That(result, Is.EqualTo("newToken"));
    }

    [Test]
    public void IsActive_ReturnsTrueIfTokenIsNotExpired()
    {
        // Arrange
        var repository = new TokenRepository(Context);
        string userId = "testUser";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "activeToken", Created = DateTime.UtcNow, Expires = DateTime.UtcNow.AddDays(1) } // Token udløber i fremtiden
        };

        Context.Users.Add(new User { Id = userId, RefreshTokens = tokens });
        Context.SaveChanges();

        // Act
        bool isActive = repository.IsActive(userId);

        // Assert
        Assert.That(isActive, Is.True);
    }

    [Test]
    public void IsActive_ReturnsFalseIfTokenIsExpired()
    {
        // Arrange
        var repository = new TokenRepository(Context);
        string userId = "testUser";
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "expiredToken", Created = DateTime.UtcNow.AddDays(-2), Expires = DateTime.UtcNow.AddDays(-1) } // Token er udløbet
        };

        Context.Users.Add(new User { Id = userId, RefreshTokens = tokens });
        Context.SaveChanges();

        // Act
        bool isActive = repository.IsActive(userId);

        // Assert
        Assert.That(isActive, Is.False);
    }


    
}