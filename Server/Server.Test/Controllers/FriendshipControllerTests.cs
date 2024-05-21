using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Server.API.Controllers;
using Server.API.Models;
using Server.API.Repositories.Interfaces;
namespace Server.Test;

public class FriendshipControllerTests : TestBase
{
    private FriendshipController _controller;
    private ILogger<FriendshipController> _logger;
    private IFriendsRepository _friendsRepository;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<FriendshipController>>();
        _friendsRepository = Substitute.For<IFriendsRepository>();
        
        _friendsRepository.AddFriendRequest("user1", "user2").Returns(Task.CompletedTask);

        _controller = new FriendshipController(Context, _friendsRepository, _logger);

    }

    [Test]
    public async Task SendFriendRequest_ReturnsOk_WhenFriendRequestSentSuccessfully()
    {
        string userId = "user1";
        string friendId = "user2";

        var result = await _controller.SendFriendRequest(userId, friendId) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task AcceptFriendRequest_ReturnsNotFound_WhenFriendRequestNotFound()
    {
        string userId = "user1";
        string friendId = "user2";
        Friendship friendship = null;
        Context.SaveChanges();

        var result = await _controller.AcceptFriendRequest(userId, friendId) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task AcceptFriendRequest_ReturnsOk_WhenFriendRequestAcceptedSuccessfully()
    {
        // Arrange
        string userId = "user1";
        string friendId = "user2";
        Friendship friendship = new Friendship { User1Id = friendId, User2Id = userId, Status = "Pending" };
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        var result = await _controller.AcceptFriendRequest(userId, friendId) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task AcceptFriendRequest_ReturnsInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        string userId = "user1";
        string friendId = "user2";
        var exceptionMessage = "Some error occurred";

        var friendshipDbSetMock = Substitute.For<DbSet<Friendship>, IQueryable<Friendship>>();
        ((IQueryable<Friendship>)friendshipDbSetMock).Provider.Returns(_ => throw new Exception(exceptionMessage));

        // Setup for exception
        //Context.Friendships.Add(Arg.Any<Friendship>()).Throws(new Exception(exceptionMessage));
        Context.Friendships = friendshipDbSetMock;

        // Act
        var result = await _controller.AcceptFriendRequest(userId, friendId) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
        Assert.That(result.Value, Is.EqualTo($"Internal server error: {exceptionMessage}"));
    }
}