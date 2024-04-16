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

[TestFixture]
public class UserControllerTests : TestBase
{
    private UserManager<User>? _subUserManager;
    private ILogger<UserController>? _subLogger;

    [SetUp]
    public void Setup()
    {
        var subUser = Substitute.For<IUserStore<User>>();
        _subUserManager = Substitute.For<UserManager<User>>(subUser, null, null, null, null, null, null, null, null);
        _subLogger = Substitute.For<ILogger<UserController>>();

        // Konfigurerer substitute til at returnere success, når CreateAsync kaldes
        _subUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Success));
    }

    /*[Test]
    public async Task MakeNewUser_CreatesUser_ReturnsOk()
    {
        // Arrange
        var controller = new UserController(Context!, _subUserManager!, _subLogger!);
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
        if (_subUserManager is IDisposable disposableManager)
        {
            disposableManager.Dispose();
        }
    }

}