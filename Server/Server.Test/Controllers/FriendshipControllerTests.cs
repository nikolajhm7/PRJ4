using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.Models;

namespace Server.Test;

public class FriendshipControllerTests : TestBase
{
    private FriendshipController _controller;
    private ILogger<FriendshipController> _logger;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<FriendshipController>>();
        _controller = new FriendshipController(Context, _logger);
    }

    [Test]
    public async Task SendFriendRequest_ReturnsNotFound_WhenUserNotFound()
    {
        string userId = "user1";
        string friendId = "user2";
        User friend = new User { Id = friendId };
        Context.Users.Add(friend);
        Context.SaveChanges();

        var result = await _controller.SendFriendRequest(userId, friendId);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task SendFriendRequest_ReturnsNotFound_WhenFriendNotFound()
    {
        string userId = "user1";
        string friendId = "user2";
        User user = new User { Id = userId };
        Context.Users.Add(user);
        Context.SaveChanges();

        var result = await _controller.SendFriendRequest(userId, friendId);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task SendFriendRequest_ReturnsBadRequest_WhenFriendshipAlreadyExists()
    {
        string userId = "user1";
        string friendId = "user2";
        User user = new User { Id = userId };
        User friend = new User { Id = friendId };
        Friendship friendship = new Friendship { User1Id = userId, User2Id = friendId, Status = "Accepted" };
        Context.Users.Add(user);
        Context.Users.Add(friend);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        var result = await _controller.SendFriendRequest(userId, friendId) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SendFriendRequest_ReturnsOk_WhenFriendRequestSentSuccessfully()
    {
        string userId = "user1";
        string friendId = "user2";
        User user = new User { Id = userId };
        User friend = new User { Id = friendId };
        Context.Users.Add(user);
        Context.Users.Add(friend);
        Context.SaveChanges();

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
        string userId = "user1";
        string friendId = "user2";
        Friendship friendship = new Friendship { User1Id = friendId, User2Id = userId, Status = "Pending" };
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        var result = await _controller.AcceptFriendRequest(userId, friendId) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
    }
}