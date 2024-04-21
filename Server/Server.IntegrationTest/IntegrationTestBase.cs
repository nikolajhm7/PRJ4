using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

        if (Context != null)
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Find den eksisterende DbContext registrering og fjern den
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Anvend den allerede oprettede DbContext konfiguration
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");  // Brug samme navn som i IntegrationTestBase for konsistens
            });

            // Tilføj yderligere mock services eller ændringer til din konfiguration her hvis nødvendigt
        });
    }
}