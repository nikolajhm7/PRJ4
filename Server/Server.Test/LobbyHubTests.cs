using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Hubs;
using Server.API.Services;
using NUnit.Framework;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.Http;

namespace Server.Test
{
    [TestFixture]
    public class LobbyHubTests
    {
        private LobbyHub _uut;
        private IHubCallerClients _clients;
        private IGroupManager _groups;
        private HubCallerContext _context;

        private ILogger<LobbyHub> _logger;
        private IIdGenerator _idGen;
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


            _logger = Substitute.For<ILogger<LobbyHub>>();
            _idGen = Substitute.For<IIdGenerator>();

            _uut = new LobbyHub(_logger, _idGen)
            {
                Clients = _clients,
                Groups = _groups,
                Context = _context
            };
        }

        [Test]
        public async Task CreateLobby_UserAuthenticated_LobbyCreatedAndUserJoined()
        {
            string lobbyId = "123456";
            _context.User?.Identity?.Name.Returns("testuser");
            _context.ConnectionId.Returns("connection-id");
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);

            var result = await _uut.CreateLobby();

            Assert.That(lobbyId, Is.EqualTo(result.Msg));
            Assert.That(result.Success, Is.True);
            await _groups.Received(1).AddToGroupAsync("connection-id", lobbyId);
        }

        [Test]
        public async Task CreateLobby_UserNotAuthenticated_ReturnsError()
        {
            _context.User?.Identity?.Name.Returns((string)null);

            var result = await _uut.CreateLobby();

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
            await _groups.DidNotReceive().AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task JoinLobby_LobbyExists_UserJoins()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _clients.Caller.Returns(_singleClientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            _clients.ClearReceivedCalls();
            _groups.ClearReceivedCalls();

            // Act
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);

            var result = await _uut.JoinLobby(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);

            //await _clientProxy.Received(1).SendAsync("UserJoinedLobby", Arg.Any<ConnectedUserDTO>());
            await _clientProxy.Received(1).SendCoreAsync("UserJoinedLobby", Arg.Any<object[]>());
            await _singleClientProxy.Received(1).SendCoreAsync("UserJoinedLobby", Arg.Any<object[]>());

            await _groups.Received(1).AddToGroupAsync(connection, lobbyId);
        }

        [Test]
        public async Task JoinLobby_LobbyDoesNotExist_ReturnsError()
        {
            string lobbyId = "nonexistent";
            _context.User?.Identity?.Name.Returns("testuser");

            var result = await _uut.JoinLobby(lobbyId);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        }

        [Test]
        public async Task LeaveLobby_LobbyExists_UserLeavesSuccessfully()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            // Add user to lobby
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            await _uut.JoinLobby(lobbyId);

            _clients.ClearReceivedCalls();
            _groups.ClearReceivedCalls();
            _clientProxy.ClearReceivedCalls();

            // Act
            var result = await _uut.LeaveLobby(lobbyId);


            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);

            await _clientProxy.Received(1).SendCoreAsync("UserLeftLobby", Arg.Any<object[]>());
            await _groups.Received(1).RemoveFromGroupAsync(connection, lobbyId);
        }

        [Test]
        public async Task LeaveLobby_LobbyDoesNotExist_ReturnsError()
        {
            var result = await _uut.LeaveLobby("nonExistingLobbyId");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        }

        [Test]
        public async Task StartGame_IsHostAndLobbyExists_ShouldStartGame()
        {
            // Arrange
            string lobbyId = "123456";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            // Act
            var result = await _uut.StartGame(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);

            await _clientProxy.Received(1).SendAsync("GameStarted");
        }

        [Test]
        public async Task StartGame_IsNotHostAndLobbyExists_NotStartGame()
        {
            // Arrange
            string lobbyId = "123456";
            var user = "user";
            var connection = "connection-id";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            // User join lobby
            _context.User?.Identity?.Name.Returns(user);
            _context.ConnectionId.Returns(connection);
            await _uut.JoinLobby(lobbyId);

            // Act
            var result = await _uut.StartGame(lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);

            await _clientProxy.DidNotReceive().SendAsync("GameStarted");
        }

        [Test]
        public async Task StartGame_LobbyDoesNotExist_ReturnsError()
        {
            var result = await _uut.StartGame("nonHostLobbyId");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        }

        [Test]
        public async Task OnDisconnectedAsync_UserInLobby_ShouldRemoveUser()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            // Add user to lobby
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            await _uut.JoinLobby(lobbyId);

            _clients.ClearReceivedCalls();
            _groups.ClearReceivedCalls();
            _clientProxy.ClearReceivedCalls();

            // Act
            await _uut.OnDisconnectedAsync(null);

            // Assert
            await _groups.Received(1).RemoveFromGroupAsync(connection, lobbyId);
            await _clientProxy.Received(1).SendCoreAsync("UserLeftLobby", Arg.Any<object[]>());
        }

        [Test]
        public async Task OnDisconnectedAsync_UserInLobbyIsHost_ShouldCloseLobby()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var hostuser = "hostuser";
            var hostconnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);

            // Create lobby
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);
            _idGen.GenerateRandomLobbyId().Returns(lobbyId);
            await _uut.CreateLobby();

            // Add user to lobby
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            await _uut.JoinLobby(lobbyId);

            // Set host to disconnect
            _context.User?.Identity?.Name.Returns(hostuser);
            _context.ConnectionId.Returns(hostconnection);

            _clients.ClearReceivedCalls();
            _groups.ClearReceivedCalls();
            _clientProxy.ClearReceivedCalls();

            // Act
            await _uut.OnDisconnectedAsync(null);


            // Assert
            await _clientProxy.Received(1).SendCoreAsync("LobbyClosed", Arg.Any<object[]>());
            await _groups.Received(2).RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        }


        [TearDown]
        public void TearDown()
        {
            _uut.lobbies.Clear();

            _uut?.Dispose();

        }
    }
}