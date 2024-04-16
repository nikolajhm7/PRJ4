using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Server.API.Data;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;

namespace Server.Test;


public class IntegrationTestBase
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
        JwtTokenService = new JwtTokenService(Configuration, TokenRepository, TimeService);

        // Setup konfigurationen
        Configuration["Jwt:Key"].Returns("3430350919ced15913aa218deb904200230f035cfcba33e4602ec193db8b6379");
        Configuration["Jwt:Issuer"].Returns("PartyPlayPalaceAPI");
        Configuration["Jwt:Audience"].Returns("PartyPlayPalaceAPI");
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
