using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Hubs;
using Server.API.Repositories.Interfaces;
using Server.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.API.DTO;

namespace Server.Test.Hubs
{
    public class FriendsHubTests
    {
        private FriendsHub _uut;
        private IHubCallerClients _clients;
        private IGroupManager _groups;
        private HubCallerContext _context;

        private ILogger<FriendsHub> _logger;
        private IFriendsRepository _friendsRepository;
        private IClientProxy _clientProxy;
        private ISingleClientProxy _singleClientProxy;

        [SetUp]
        public void Setup()
        {

            _clients = Substitute.For<IHubCallerClients>();
            _groups = Substitute.For<IGroupManager>();
            _context = Substitute.For<HubCallerContext>();
            _clientProxy = Substitute.For<IClientProxy>();
            _singleClientProxy = Substitute.For<ISingleClientProxy>();

            _friendsRepository = Substitute.For<IFriendsRepository>();
            _logger = Substitute.For<ILogger<FriendsHub>>();

            _uut = new FriendsHub(_logger, _friendsRepository)
            {
                Clients = _clients,
                Groups = _groups,
                Context = _context
            };
        }

        [Test]
        public async Task SendFriendRequest_UserIsNull_ReturnsFailedActionResult()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string)null);

            // Act
            var result = await _uut.SendFriendRequest("otherUser");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }

        [Test]
        public async Task SendFriendRequest_ValidRequest_InvokesRepositoryAndClients()
        {
            // Arrange
            var otherUsername = "otherUser";
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);
            _clients.Group(otherUsername).Returns(_clientProxy);

            // Act
            var result = await _uut.SendFriendRequest(otherUsername);

            // Assert
            await _friendsRepository.Received(1).AddFriendRequest(username, otherUsername);
            await _clientProxy.Received(1).SendCoreAsync("NewFriendRequest", Arg.Any<object[]>());
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
        }

        [Test]
        public async Task AcceptFriendRequest_UserIsNull_ReturnsFailedActionResult()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string)null);

            // Act
            var result = await _uut.AcceptFriendRequest("otherUser");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }

        [Test]
        public async Task AcceptFriendRequest_ValidRequest_UpdatesRepositoryAndNotifiesClients()
        {
            // Arrange
            var otherUsername = "otherUser";
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);
            _clients.Group(otherUsername).Returns(_clientProxy);

            // Act
            var result = await _uut.AcceptFriendRequest(otherUsername);

            // Assert
            await _friendsRepository.Received(1).AcceptFriendRequest(username, otherUsername);
            await _clientProxy.Received().SendCoreAsync("FriendRequestAccepted", Arg.Any<object[]>());
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
        }

        [Test]
        public async Task RemoveFriend_UserIsNull_ReturnsFailedActionResult()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string)null);

            // Act
            var result = await _uut.RemoveFriend("otherUser");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }

        [Test]
        public async Task RemoveFriend_ValidRequest_UpdatesRepositoryAndNotifiesClients()
        {
            // Arrange
            var otherUsername = "otherUser";
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);
            _clients.Group(otherUsername).Returns(_clientProxy);

            // Act
            var result = await _uut.RemoveFriend(otherUsername);

            // Assert
            await _friendsRepository.Received(1).RemoveFriend(username, otherUsername);
            await _clientProxy.Received().SendCoreAsync("FriendRemoved", Arg.Any<object[]>());
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
        }

        [Test]
        public async Task InviteFriend_ValidRequest_NotifiesClient()
        {
            // Arrange
            var otherUsername = "otherUser";
            _clients.Group(otherUsername).Returns(_clientProxy);

            // Act
            var result = await _uut.InviteFriend(otherUsername);

            // Assert
            await _clientProxy.Received().SendCoreAsync("NewGameInvite", Arg.Any<object[]>());
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
        }

        [Test]
        public async Task InviteFriend_UserIsNull_ReturnsFailedActionResult()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string)null);

            // Act
            var result = await _uut.InviteFriend("otherUser");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task GetFriends_UserIsNull_ReturnsFailedActionResult()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string)null);

            // Act
            var result = await _uut.GetFriends(true);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task GetFriends_ValidRequest_ReturnsFriends()
        {
            // Arrange
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);
            var friends = new List<FriendDTO>();
            _friendsRepository.GetFriendsOf(username).Returns(friends);

            // Act
            var result = await _uut.GetFriends(false);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
            Assert.That(result.Value, Is.EqualTo(friends));
        }
        
        [Test]
        public async Task GetFriends_ValidRequestWithInvites_ReturnsFriendsAndInvites()
        {
            // Arrange
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);
            var friends = new List<FriendDTO>();
            var invites = new List<FriendDTO>();
            _friendsRepository.GetFriendsOf(username).Returns(friends);
            _friendsRepository.GetInvitesOf(username).Returns(invites);

            // Act
            var result = await _uut.GetFriends(true);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
            Assert.That(result.Value, Is.EqualTo(friends.Concat(invites)));
        }
        
        [Test]
        public async Task OnConnectedAsync_ValidRequest_JoinsGroup()
        {
            // Arrange
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);

            // Act
            await _uut.OnConnectedAsync();

            // Assert
            await _groups.Received().AddToGroupAsync(_context.ConnectionId, username);
        }
        
        [Test]
        public async Task OnDisconnectedAsync_ValidRequest_LeavesGroup()
        {
            // Arrange
            var username = "user";
            _context.User?.Identity?.Name.Returns(username);

            // Act
            await _uut.OnDisconnectedAsync(new Exception());

            // Assert
            await _groups.Received().RemoveFromGroupAsync(_context.ConnectionId, username);
        }

        [TearDown]
        public void TearDown()
        {
            _uut?.Dispose();
        }
    }
}
