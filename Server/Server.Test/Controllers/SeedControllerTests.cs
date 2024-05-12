using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.Test;

public class SeedControllerTests : TestBase
{
    private ILogger<SeedController> _mockLogger;
    private IGameRepository _mockGameRepository;
    private IUserRepository _mockUserRepository;
    
    [SetUp]
    public void Setup()
    {
        _mockLogger = Substitute.For<ILogger<SeedController>>();
        _mockGameRepository = Substitute.For<IGameRepository>();
        _mockUserRepository = Substitute.For<IUserRepository>();
    }

    [Test]
    public async Task SeedData_ShouldReturnOk_WhenSeedingIsSuccessful()
    {
        // Arrange
        var user = new User { Id = "1", UserName = "Hugo123", Email = "Hugo123@gmail.com" };
        var game = new Game { GameId = 1, Name = "hangman" };

        var identityResult = IdentityResult.Success;

        _mockUserRepository.AddUser(Arg.Is<User>(u => u.UserName == "Hugo123" && u.Email == "Hugo123@gmail.com"), "Hugo123Hugo123")
            .Returns(Task.FromResult(identityResult));
        _mockGameRepository.AddGame(game).Returns(Task.CompletedTask);
        _mockGameRepository.AddGameToUser(user.Id, game.GameId).Returns(Task.CompletedTask);
        
        SeedController controller = new SeedController(_mockLogger, _mockGameRepository, _mockUserRepository);
        
        // Act
        var result = await controller.SeedData();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo("Seeding completed successfully."));
    }

    [Test]
    public async Task SeedData_ShouldReturnBadRequest_WhenUserSeedingFails()
    {
        // Arrange
        var user = new User { UserName = "Hugo123", Email = "Hugo123@gmail.com" };
        var identityErrors = new[] { new IdentityError { Description = "Error" } };
        var identityResult = IdentityResult.Failed(identityErrors);

        _mockUserRepository.AddUser(Arg.Is<User>(u => u.UserName == "Hugo123" && u.Email == "Hugo123@gmail.com"), "Hugo123Hugo123")
            .Returns(Task.FromResult(identityResult));
        
        SeedController controller = new SeedController(_mockLogger, _mockGameRepository, _mockUserRepository);
        
        // Act
        var result = await controller.SeedData();

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}