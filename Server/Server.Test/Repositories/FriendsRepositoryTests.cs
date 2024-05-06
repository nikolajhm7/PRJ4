using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;

namespace Server.Test.Repositories;

public class FriendsRepositoryTests : TestBase
{
    private FriendsRepository _friendsRepository;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        _friendsRepository = new FriendsRepository(Context);
    }

    [Test]
    public void AddFriendRequest_AddsRequestWhenBothUsersExistAndDifferent()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        Context.Users.Add(user1);
        Context.Users.Add(user2);
        Context.SaveChanges();

        // Act
        _friendsRepository.AddFriendRequest(user1.UserName, user2.UserName);

        // Assert
        var request = Context.Friendships.FirstOrDefault();
        Assert.That(request, Is.Not.Null);
        Assert.That(request.Status, Is.EqualTo("Pending"));
    }

    [Test]
    public void AddFriendRequest_DoesNothingWhenUsersAreSame()
    {
        // Arrange
        var user = new User { Id = "user1" };
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        _friendsRepository.AddFriendRequest(user.Id, user.Id);

        // Assert
        var requests = Context.Friendships.ToList();
        Assert.That(requests, Is.Empty);
    }

    [Test]
    public void AcceptFriendRequest_ChangesStatusToAccepted()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        var friendship = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Pending", User1 = user1, User2 = user2 };
        Context.Users.AddRange(user1, user2);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        _friendsRepository.AcceptFriendRequest(user2.UserName, user1.UserName);

        // Assert
        var updatedFriendship = Context.Friendships.FirstOrDefault();
        Assert.That(updatedFriendship, Is.Not.Null);
        Assert.That(updatedFriendship.Status, Is.EqualTo("Accepted"));
    }

    [Test]
    public void RemoveFriend_RemovesFriendship()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        var friendship = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Accepted", User1 = user1, User2 = user2 };
        Context.Users.AddRange(user1, user2);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        _friendsRepository.RemoveFriend(user1.UserName, user2.UserName);

        // Assert
        var remainingFriendships = Context.Friendships.ToList();
        Assert.That(remainingFriendships, Is.Empty);
    }

    [Test]
    public async Task GetFriendsOf_ReturnsCorrectFriends()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        var friendship = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Accepted", date = DateTime.Now };
        Context.Users.AddRange(user1, user2);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        var friends = await _friendsRepository.GetFriendsOf(user1.UserName);

        // Assert
        Assert.That(friends.Count, Is.EqualTo(1));
        Assert.That(friends[0].Name, Is.EqualTo(user2.UserName));
        Assert.That(friends[0].FriendsSince, Is.EqualTo(friendship.date));
    }

    [Test]
    public async Task AddFriendRequest_ThrowsExceptionWhenUserNotFound()
    {
        // Act
        Assert.ThrowsAsync<Exception>(() => _friendsRepository.AddFriendRequest("user1", "user2"));
    }

    [Test]
    public async Task AddFriendRequest_ThrowsExceptionWhenFriendNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        Assert.ThrowsAsync<Exception>(() => _friendsRepository.AddFriendRequest(user.Id, "user2"));
    }

    [Test]
    public async Task RemoveFriend_ThrowsExceptionWhenFriendshipNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        Assert.ThrowsAsync<Exception>(() => _friendsRepository.RemoveFriend(user.Id, "user2"));
    }

    [Test]
    public async Task AcceptFriendRequest_ThrowsExceptionWhenFriendshipNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
       Assert.ThrowsAsync<Exception>(() => _friendsRepository.AcceptFriendRequest(user.UserName, "user2"));
    }

    [Test]
    public async Task AcceptFriendRequest_ThrowsExceptionWhenFriendshipNotPending()
    {
        // Arrange
        var user1 = new User { Id = "user1" };
        var user2 = new User { Id = "user2" };
        var friendship = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Accepted" };
        Context.Users.AddRange(user1, user2);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        Assert.ThrowsAsync<Exception>(() => _friendsRepository.AcceptFriendRequest(user2.UserName, user1.UserName));
    }


    [Test]
    public async Task GetFriendsOf_ReturnsFriendsForBothFriends1AndFriends2()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        var user3 = new User { Id = "user3", UserName = "User3" };
        var friendship1 = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Accepted", date = DateTime.Now, User1 = user1, User2 = user2 };
        var friendship2 = new Friendship { User1Id = user3.Id, User2Id = user1.Id, Status = "Accepted", date = DateTime.Now, User1 = user3, User2 = user1 };
        Context.Users.AddRange(user1, user2, user3);
        Context.Friendships.AddRange(friendship1, friendship2);
        Context.SaveChanges();

        // Act
        var friends = await _friendsRepository.GetFriendsOf(user1.UserName);

        // Assert
        Assert.That(friends.Count, Is.EqualTo(2));
        Assert.That(friends[0].Name, Is.EqualTo(user2.UserName));
        Assert.That(friends[1].Name, Is.EqualTo(user3.UserName));
    }
    
    [Test]
    public async Task GetInvitesOf_ReturnsCorrectInvites()
    {
        // Arrange
        var user1 = new User { Id = "user1", UserName = "User1" };
        var user2 = new User { Id = "user2", UserName = "User2" };
        var friendship = new Friendship { User1Id = user1.Id, User2Id = user2.Id, Status = "Pending", date = DateTime.Now, User1 = user1, User2 = user2 };
        
        Context.Users.AddRange(user1, user2);
        Context.Friendships.Add(friendship);
        Context.SaveChanges();

        // Act
        var invites = await _friendsRepository.GetInvitesOf(user2.UserName);

        // Assert
        Assert.That(invites.Count, Is.EqualTo(1));
        Assert.That(invites[0].Name, Is.EqualTo(user1.UserName));
        Assert.That(invites[0].FriendsSince, Is.EqualTo(friendship.date));
    }
    
    [Test]
    public async Task AddFriendRequest_ThrowsExceptionWhenFriendUserNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        Assert.ThrowsAsync<Exception>(() => _friendsRepository.AddFriendRequest(user.UserName, "user2"));
    }

}
