using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Server.API.Models;
using Server.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Server.API.Data;
using Server.API.DTO;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;

namespace Server.IntegrationTests;

public class IT5_GameControllerJwtTokenServiceGameRepositoryUserRepository : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [SetUp]
    public void SetUp()
    {
        _factory = new SpecializedWebApplicationFactory();
        _client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    }

    [Test]
    public async Task GetGamesForUser_ReturnsGames()
    {
        // Arrange
        var username = "testUser";
        
        var user = new User
        {
            Id = "1",
            UserName = username
        };
        
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        
        var game1 = new Game
        {
            GameId = 1,
            Name = "testGame",
        };
        
        var game2 = new Game
        {
            GameId = 2,
            Name = "testGame2",
        };
        
        await Context.Games.AddRangeAsync(game1, game2);
        await Context.SaveChangesAsync();
        
        var userGame1 = new UserGame
        {
            UserId = user.Id,
            GameId = game1.GameId
        };
        
        var userGame2 = new UserGame
        {
            UserId = user.Id,
            GameId = game2.GameId
        };
        
        await Context.UserGames.AddRangeAsync(userGame1, userGame2);
        await Context.SaveChangesAsync();
        
        
        var token = JwtTokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/Game/getGamesForUser/{username}"); // Ændre til den rigtige URL
        
        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var games = await response.Content.ReadAsAsync<List<Game>>();
        Assert.That(games, Is.Not.Empty);
    }
    
    [Test]
    public async Task AddGameForUser_ReturnsOk()
    {
        // Arrange
        var username = "testUser";
        
        var user = new User
        {
            Id = "1",
            UserName = username
        };
        
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        
        var game = new Game
        {
            GameId = 1,
            Name = "testGame",
        };
        
        await Context.Games.AddAsync(game);
        await Context.SaveChangesAsync();
        
        var token = JwtTokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var gameUserDto = new GameUserDTO
        {
            GameId = game.GameId,
            UserName = username
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/Game/addGameForUser")
        {
            Content = new StringContent(JsonConvert.SerializeObject(gameUserDto), Encoding.UTF8, "application/json")
        };
        
        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var userGame = await Context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == user.Id && ug.GameId == game.GameId);
        Assert.That(userGame, Is.Not.Null);
        
    }
    
    [Test]
    public async Task AddGame_ReturnsOk()
    {
        // Arrange
        var username = "testUser";
        
        var user = new User
        {
            Id = "1",
            UserName = username
        };
        
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        
        var token = JwtTokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var gameName = "testGame";
        
        var gameDto = new GameDTO
        {
            Name = gameName,
            MaxPlayers = 4
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/Game/addGame")
        {
            Content = new StringContent(JsonConvert.SerializeObject(gameDto), Encoding.UTF8, "application/json")
        };
        
        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var addedGame = await Context.Games.FirstOrDefaultAsync(g => g.Name == gameName);
        Assert.That(addedGame, Is.Not.Null);
    }
    
    [Test]
    public async Task EditGame_ReturnsOk()
    {
        // Arrange
        var username = "testUser";
        
        var user = new User
        {
            Id = "1",
            UserName = username
        };
        
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        
        var token = JwtTokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var game = new Game
        {
            GameId = 1,
            Name = "testGame",
            MaxPlayers = 4
        };
        
        await Context.Games.AddAsync(game);
        await Context.SaveChangesAsync();
        
        var gameName = "testGame2";
        
        var gameDto = new GameDTO
        {
            Name = gameName,
            MaxPlayers = 6
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"/Game/editGame/{game.GameId}")
        {
            Content = new StringContent(JsonConvert.SerializeObject(gameDto), Encoding.UTF8, "application/json")
        };
        
        // Act
        var response = await _client!.SendAsync(request);

        await Context.Entry(game).ReloadAsync();
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var editedGame = await Context.Games.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        Assert.That(editedGame, Is.Not.Null);
        Assert.That(editedGame.Name, Is.EqualTo(gameName));
        Assert.That(editedGame.MaxPlayers, Is.EqualTo(6));
    }
    
    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
}

public class SpecializedWebApplicationFactory : CustomWebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder); // Kald til basisklassens konfiguration

        builder.ConfigureServices(services =>
        {
            // Fjern eksisterende tjenester, der skal substitueres
            var userManagerDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(UserManager<User>));
            if (userManagerDescriptor != null)
            {
                services.Remove(userManagerDescriptor);
            }

            // Tilføj en substitut for UserManager
            var userManagerSub = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);
            userManagerSub.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(new User { UserName = "testUser", Id = "1" }));

            services.AddSingleton<UserManager<User>>(userManagerSub);

        });
    }

}
