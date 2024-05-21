using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Server.API.Models;
using Server.API.Controllers;
using Server.API.Data;
using Server.API.DTO;
using NSubstitute;
using Server.API.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Server.Test;

public class UserControllerTests
{
    private UserController _controller;
    private IUserRepository _userRepository;
    private ILogger<UserController> _logger;
    private ApplicationDbContext _context;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserController>>();

        _controller = new UserController(_logger, _userRepository, _context)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
    
    [Test]
    public async Task MakeNewUser_ReturnsBadRequest_WhenModelValidationFails()
    {
        _controller.ModelState.AddModelError("UserName", "Username is required");

        var result = await _controller.MakeNewUser(new RegisterDto()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        
        var errorResponse = JObject.FromObject(result.Value);
        var errors = errorResponse["errors"].ToObject<List<string>>();
        
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors, Does.Contain("Username is required"));
    }

    [Test]
    public async Task MakeNewUser_ReturnsBadRequest_WhenUserCreationFails()
    {
        var registerDto = new RegisterDto { UserName = "testUser", Email = "test@example.com", Password = "Password123!" };
        var identityErrors = new List<IdentityError> { new IdentityError { Description = "Email already exists." } };
        _userRepository.AddUser(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Failed(identityErrors.ToArray()));

        var result = await _controller.MakeNewUser(registerDto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        
        var errorResponse = JObject.FromObject(result.Value);
        var errors = errorResponse["errors"].ToObject<List<string>>();
        
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors, Does.Contain("Email already exists."));
    }


    [Test]
    public async Task MakeNewUser_ReturnsOk_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var registerDto = new RegisterDto { UserName = "testUser", Email = "test@example.com", Password = "Password123!" };
        _userRepository.AddUser(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        // Act
        var actionResult = await _controller.MakeNewUser(registerDto);
        var okResult = actionResult as OkObjectResult;

        // Assert
        Assert.That(okResult, Is.Not.Null);

        // Opretter et anonymt objekt, der matcher strukturen af det objekt, der returneres af handlingen
        var errorResponse = JObject.FromObject(okResult.Value);

        // Tilgå 'message' property og udfør assertion
        var messageProperty = errorResponse["message"].ToObject<string>();
        Assert.That(messageProperty, Is.Not.Null);
        Assert.That(messageProperty, Does.Contain("User created successfully"));
        
    }

    [Test]
    public async Task GetUserIdFromUsername_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var username = "testUser";
        var user = new User { Id = "userId", UserName = username };
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var context = new ApplicationDbContext(dbContextOptions))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var logger = Substitute.For<ILogger<UserController>>();
        var userRepository = Substitute.For<IUserRepository>();
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var context = new ApplicationDbContext(contextOptions))
        {
            var controller = new UserController(logger, userRepository, context);

            // Act
            var result = await controller.GetUserIdFromUsername(username);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(user.Id));
        }
    }

    [Test]
    public async Task GetUserIdFromUsername_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var username = "nonExistingUser";
        var logger = Substitute.For<ILogger<UserController>>();
        var userRepository = Substitute.For<IUserRepository>();
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var context = new ApplicationDbContext(contextOptions))
        {
            var controller = new UserController(logger, userRepository, context);

            // Act
            var result = await controller.GetUserIdFromUsername(username);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
    }

    [Test]
    public async Task PasswordAttributeTest()
    {
        // Arrange
        var attribute = new PasswordRequirementsAttribute();

        // Act & Assert
        // Test password without uppercase letter

        var result1 = attribute.IsValid("password123");
        Assert.That(result1, Is.EqualTo(false));

        // Test password without lowercase letter
        var result2 = attribute.IsValid("PASSWORD123");
        Assert.That(result2, Is.EqualTo(false));

        // Test password without digit
        var result3 = attribute.IsValid("Password");
        Assert.That(result3, Is.EqualTo(false));

        // Test password with all requirements met
        var result4 = attribute.IsValid("Password123");
        Assert.That(result4, Is.EqualTo(true));

        // Test password with whitespace
        var result5 = attribute.IsValid("");
        Assert.That(result5, Is.EqualTo(false));

        // Test password as null
        var result6 = attribute.IsValid(null);
        Assert.That(result5, Is.EqualTo(false));
    }

}
