using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Server.API.Data;
using Server.API.DTO;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using Server.Test;

namespace Server.IntegrationTests;

public class IT3_LobbyManagerGameRepositoryRandomGenerator : IntegrationTestBase
{
    private LobbyManager _lobbyManager;
    private IServiceScope _serviceScope;
    private IServiceProvider _serviceProvider;
    private ServiceCollection _services;
    private IIdGenerator _idGenerator;


    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
    
        _services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
    
        _services.AddScoped<IGameRepository, GameRepository>();
        _services.AddScoped<IUserRepository, UserRepository>();
        
        _services.AddScoped<IUserStore<User>>(provider => new UserStore<User>(provider.GetRequiredService<ApplicationDbContext>()));
        _services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 10;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        });
        _services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        _services.AddScoped<IUserValidator<User>, UserValidator<User>>();
        _services.AddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
        _services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        _services.AddScoped<IdentityErrorDescriber>();
        
        _services.AddLogging();
        
        _services.AddScoped<UserManager<User>>();
        _services.AddSingleton<IIdGenerator, RandomGenerator>();

        var rootProvider = _services.BuildServiceProvider();
        _serviceScope = rootProvider.CreateScope(); // Opretter en scope
        _serviceProvider = _serviceScope.ServiceProvider;
    
        _idGenerator = new RandomGenerator();
    
        _lobbyManager = new LobbyManager(_idGenerator, _serviceProvider);
    }
    
    [Test]
    public async Task CreateNewLobby_ShouldCreateLobby_WhenGameExists()
    {
        Context.Games.Add(new Game { GameId = 1, MaxPlayers = 5, Name = "TestGame" }); 
        Context.SaveChanges();

        var user = new ConnectedUserDTO("username", "connectionId");

        // Act
        var lobbyId = await _lobbyManager.CreateNewLobby(user, 1);

        // Assert
        Assert.That(lobbyId, Is.Not.Null);
        Assert.That(_lobbyManager.lobbies.ContainsKey(lobbyId), Is.True);
    }

    
    [TearDown]
    public void TearDown()
    {
        _serviceScope?.Dispose();
    }
    
}