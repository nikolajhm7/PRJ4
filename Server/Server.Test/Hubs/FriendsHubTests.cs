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

            // Act
            var result = await _uut.SendFriendRequest(otherUsername);

            // Assert
            await _friendsRepository.Received(1).AddFriendRequest(username, otherUsername);
            await _clients.User(otherUsername).Received(1).SendCoreAsync("NewFriendRequest", Arg.Any<object[]>());
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

            // Act
            var result = await _uut.AcceptFriendRequest(otherUsername);

            // Assert
            await _friendsRepository.Received(1).AcceptFriendRequest(username, otherUsername);
            await _clients.User(otherUsername).Received().SendCoreAsync("FriendRequestAccepted", Arg.Any<object[]>());
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

            // Act
            var result = await _uut.RemoveFriend(otherUsername);

            // Assert
            await _friendsRepository.Received(1).RemoveFriend(username, otherUsername);
            await _clients.User(otherUsername).Received().SendCoreAsync("FriendRemoved", Arg.Any<object[]>());
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
        }

        [Test]
        public async Task InviteFriend_ValidRequest_NotifiesClient()
        {
            // Arrange
            var otherUsername = "otherUser";

            // Act
            var result = await _uut.InviteFriend(otherUsername);

            // Assert
            await _clients.User(otherUsername).Received().SendCoreAsync("NewGameInvite", Arg.Any<object[]>());
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

        [TearDown]
        public void TearDown()
        {
            _uut?.Dispose();
        }
    }
}
