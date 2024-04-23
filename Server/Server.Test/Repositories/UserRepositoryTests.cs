using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Repository.Interfaces;

namespace Server.Test.Repositories;

public class UserRepositoryTests
{
    private IUserRepository _userRepository;
    private UserManager<User> _userManager;

    [SetUp]
    public void Setup()
    {
        _userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(), 
            null, null, null, null, null, null, null, null);

        _userRepository = new UserRepository(_userManager);
    }
    
    [Test]
    public async Task GetUserByName_ReturnsUserWhenExists()
    {
        // Arrange
        var userName = "testUser";
        var expectedUser = new User { UserName = userName };
        _userManager.FindByNameAsync(userName).Returns(Task.FromResult(expectedUser));

        // Act
        var result = await _userRepository.GetUserByName(userName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedUser));
    }
    
    [Test]
    public async Task AddUser_CreatesNewUserSuccessfully()
    {
        // Arrange
        var user = new User { UserName = "newUser" };
        var password = "Password123!";
        _userManager.CreateAsync(user, password).Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await _userRepository.AddUser(user, password);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(IdentityResult.Success));
    }

    [Test]
    public async Task RemoveUser_DeletesUserSuccessfully()
    {
        // Arrange
        var user = new User { UserName = "deleteUser" };
        _userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await _userRepository.RemoveUser(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(IdentityResult.Success));
    }
    
    [Test]
    public async Task UserCheckPassword_VerifiesUserPasswordCorrectly()
    {
        // Arrange
        var user = new User { UserName = "existingUser" };
        var password = "correctPassword";
        _userManager.CheckPasswordAsync(user, password).Returns(Task.FromResult(true));

        // Act
        var result = await _userRepository.UserCheckPassword(user, password);

        // Assert
        Assert.That(result, Is.True);
    }
    
    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }
    

}
