using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.API.Models;
using Server.API.Controllers;
using Server.API.Data;
using Server.API.DTO;
using NSubstitute;

namespace Server.Test;

public class UserControllerTests
{
    private UserManager<User>? _subUserManager;
    private ILogger<UserController>? _subLogger;
    private ApplicationDbContext? _subContext;

    [SetUp]
    public void Setup()
    {
        var subUser = Substitute.For<IUserStore<User>>();
        _subUserManager = Substitute.For<UserManager<User>>(subUser, null, null, null, null, null, null, null, null);
        _subLogger = Substitute.For<ILogger<UserController>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb") // Bruger en in-memory database for tests
            .Options;
        _subContext = Substitute.For<ApplicationDbContext>(options);

        // Konfigurerer substitute til at returnere success, når CreateAsync kaldes
        _subUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Success));
    }

    /*[Test]
    public async Task MakeNewUser_CreatesUser_ReturnsOk()
    {
        // Arrange
        var controller = new UserController(_subContext!, _subUserManager!, _subLogger!);
        var newUser = new RegisterDto { UserName = "testUser", Email = "test@example.com", Password = "Password123!" };

        // Act
        var result = await controller.MakeNewUser(newUser);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _subUserManager?.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<string>());
    }*/

    [TearDown]
    public void TearDown()
    {
        if (_subContext != null)
        {
            _subContext.Database.EnsureDeleted();
            _subContext.Dispose(); // Frigør ressourcer
        }

        if (_subUserManager is IDisposable disposableManager)
        {
            disposableManager.Dispose();
        }
    }

}