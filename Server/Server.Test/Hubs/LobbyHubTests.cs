using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Hubs;
using NUnit.Framework;
using Server.API.DTO;
using Server.API.Models;
using Microsoft.AspNetCore.Http;
using Server.API.Services.Interfaces;
using Server.API.Services;
using NSubstitute.Core.Arguments;

namespace Server.Test.Hubs
{
    [TestFixture]
    public class LobbyHubTests
    {
        private LobbyHub _uut;
        private IHubCallerClients _clients;
        private IGroupManager _groups;
        private HubCallerContext _context;

        private ILogger<LobbyHub> _logger;
        private IClientProxy _clientProxy;
        private ISingleClientProxy _singleClientProxy;
        private ILobbyManager _lobbyManager;

        [SetUp]
        public void Setup()
        {

            _clients = Substitute.For<IHubCallerClients>();
            _groups = Substitute.For<IGroupManager>();
            _context = Substitute.For<HubCallerContext>();
            _clientProxy = Substitute.For<IClientProxy>();
            _singleClientProxy = Substitute.For<ISingleClientProxy>();


            _logger = Substitute.For<ILogger<LobbyHub>>();
            _lobbyManager = Substitute.For<ILobbyManager>();

            _uut = new LobbyHub(_logger, _lobbyManager)
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
            _lobbyManager.CreateNewLobby(Arg.Any<ConnectedUserDTO>(), Arg.Any<int>()).Returns(lobbyId);

            var result = await _uut.CreateLobby(1);

            Assert.That(lobbyId, Is.EqualTo(result.Msg));
            Assert.That(result.Success, Is.True);
            await _groups.Received(1).AddToGroupAsync("connection-id", lobbyId);
        }

        [Test]
        public async Task CreateLobby_UserNotAuthenticated_ReturnsError()
        {
            _context.User?.Identity?.Name.Returns((string?)null);

            var result = await _uut.CreateLobby(1);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
            await _groups.DidNotReceive().AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        //[Test]
        //public async Task JoinLobby_LobbyExists_UserJoins()
        //{
        //    // Arrange
        //    string lobbyId = "123456";
        //    var username = "testuser";
        //    var connection = "connection-id";

        //    _clients.Group(lobbyId).Returns(_clientProxy);
        //    _clients.Caller.Returns(_singleClientProxy);
        //    _lobbyManager.LobbyExists(lobbyId).Returns(true);
        //    var list = new List<ConnectedUserDTO> { new ConnectedUserDTO("", "")};
        //    var ar = new ActionResult<List<ConnectedUserDTO>>(true, null, list);
        //    _lobbyManager.AddToLobby(Arg.Any<ConnectedUserDTO>(), lobbyId).Returns(ar);
        //    _lobbyManager.GetUsersInLobby(lobbyId).Returns(list);

        //    // Act
        //    _context.User?.Identity?.Name.Returns(username);
        //    _context.ConnectionId.Returns(connection);

        //    var result = await _uut.JoinLobby(lobbyId);

        //    // Assert
        //    Assert.That(result.Success, Is.True);
        //    Assert.That(result.Msg, Is.Null);
        //    Assert.That(result.Value, Is.EqualTo(list));

        //    await _clientProxy.Received(1).SendCoreAsync("UserJoinedLobby", Arg.Any<object[]>());

        //    await _groups.Received(1).AddToGroupAsync(connection, lobbyId);
        //}

        [Test]
        public async Task JoinLobby_LobbyExistsLobbyFull_UserDoesNotJoin()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _clients.Caller.Returns(_singleClientProxy);
            _lobbyManager.LobbyExists(lobbyId).Returns(true);
            var ar = new ActionResult<List<ConnectedUserDTO>>(false, "Lobby is full.", []);
            _lobbyManager.AddToLobby(Arg.Any<ConnectedUserDTO>(), lobbyId).Returns(ar);

            // Act
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);

            var result = await _uut.JoinLobby(lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);

            await _clientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
            await _singleClientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());

            await _groups.DidNotReceive().AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task JoinLobby_LobbyDoesNotExist_ReturnsError()
        {
            string lobbyId = "nonexistent";
            _lobbyManager.LobbyExists(lobbyId).Returns(false);

            var result = await _uut.JoinLobby(lobbyId);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        }

        [Test]
        public async Task LeaveLobby_UserNotAuthenticated_ReturnsError()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string?)null);
            // Act
            var result = await _uut.LeaveLobby("");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task LeaveLobby_LobbyExists_UserLeavesSuccessfully()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _lobbyManager.LobbyExists(lobbyId).Returns(true);
            _context.ConnectionId.Returns(connection);

            // Act
            var result = await _uut.LeaveLobby(lobbyId);


            // Assert
            _lobbyManager.RemoveFromLobby(new(username, connection), lobbyId);
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
        public async Task StartGame_UserNotAuthenticated_ReturnsError()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string?)null);
            // Act
            var result = await _uut.StartGame("");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task StartGame_IsHostAndLobbyExists_ShouldStartGame()
        {
            // Arrange
            string lobbyId = "123456";
            var hostUser = "hostuser";
            var hostConnection = "host-connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _lobbyManager.IsHost(hostUser, lobbyId).Returns(true);
            _context.User?.Identity?.Name.Returns(hostUser);
            _context.ConnectionId.Returns(hostConnection);

            // Act
            var result = await _uut.StartGame(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            _lobbyManager.Received(1).StartGame(lobbyId);
            await _clientProxy.Received(1).SendAsync("GameStarted");
        }

        [Test]
        public async Task StartGame_IsNotHostAndLobbyExists_NotStartGame()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "username-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _lobbyManager.IsHost(username, lobbyId).Returns(false);

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
        public async Task OnDisconnectedAsync_UserInLobbyGameNotStarted_ShouldRemoveUser()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var user = new ConnectedUserDTO(username, connection);

            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            _clients.Group(lobbyId).Returns(_clientProxy);
            _lobbyManager.GetLobbyIdFromUsername(username).Returns(lobbyId);
            _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InLobby);
            _lobbyManager.IsHost(username, lobbyId).Returns(false);

            // Act
            await _uut.OnDisconnectedAsync(null);

            // Assert
            _lobbyManager.Received(1).RemoveFromLobby(user, lobbyId);
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
            var user = new ConnectedUserDTO(username, connection);

            _clients.Group(lobbyId).Returns(_clientProxy);
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            _lobbyManager.IsHost(username, lobbyId).Returns(true);
            _lobbyManager.GetLobbyIdFromUsername(username).Returns(lobbyId);
            _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InLobby);
            var list = new List<ConnectedUserDTO> { user, new ConnectedUserDTO("", "") };
            _lobbyManager.GetUsersInLobby(lobbyId).Returns(list);

            // Act
            await _uut.OnDisconnectedAsync(null);


            // Assert
            _lobbyManager.Received(1).RemoveLobby(lobbyId);
            await _clientProxy.Received(1).SendCoreAsync("LobbyClosed", Arg.Any<object[]>());
            await _groups.Received(2).RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task OnDisconnectedAsync_UserInLobbyGameStarted_ShouldIgnore()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";
            var user = new ConnectedUserDTO(username, connection);

            _clients.Group(lobbyId).Returns(_clientProxy);
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);
            _lobbyManager.IsHost(username, lobbyId).Returns(true);
            _lobbyManager.GetLobbyIdFromUsername(username).Returns(lobbyId);
            _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InGame);

            // Act
            await _uut.OnDisconnectedAsync(null);


            // Assert
            _lobbyManager.DidNotReceive().RemoveLobby(Arg.Any<string>());
            await _clientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
            await _groups.Received(1).RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        }
        
        [Test]
        public async Task JoinLobby_UsernameIsNull_ReturnsError()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string?)null);

            // Act
            var result = await _uut.JoinLobby("123456");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task JoinLobby_JoinedLobbySuccessfully_ReturnsSuccess()
        {
            // Arrange
            string lobbyId = "123456";
            var username = "testuser";
            var connection = "connection-id";

            _clients.Group(lobbyId).Returns(_clientProxy);
            _clients.Caller.Returns(_singleClientProxy);
            _lobbyManager.LobbyExists(lobbyId).Returns(true);
            var ar = new ActionResult<List<ConnectedUserDTO>>(true, null, new List<ConnectedUserDTO>());
            _lobbyManager.AddToLobby(Arg.Any<ConnectedUserDTO>(), lobbyId).Returns(ar);

            // Act
            _context.User?.Identity?.Name.Returns(username);
            _context.ConnectionId.Returns(connection);

            var result = await _uut.JoinLobby(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.EqualTo(lobbyId));
            
            await _clientProxy.Received(1).SendCoreAsync("UserJoinedLobby", Arg.Any<object[]>());

            await _groups.Received(1).AddToGroupAsync(connection, lobbyId);
        }
        
        [Test]
        public async Task GetLobbyGameId_ReturnsGameId()
        {
            // Arrange
            string lobbyId = "123456";
            var gameId = 1;
            _lobbyManager.GetLobbyGameId(lobbyId).Returns(new ActionResult<int>(true, null, gameId));

            // Act
            var result = await _uut.GetLobbyGameId(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
            Assert.That(result.Value, Is.EqualTo(gameId));
        }
        
        [Test]
        public async Task UserIsHost_ReturnsActionResult()
        {
            // Arrange
            string lobbyId = "123456";
            _context.User?.Identity?.Name.Returns("testuser");
            _lobbyManager.IsHost("testuser", lobbyId).Returns(true);

            // Act
            var result = await _uut.UserIsHost(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Not.Null);
        }
        
        [Test]
        public async Task UserIsHost_UserIsNotHost_ReturnsError()
        {
            // Arrange
            string lobbyId = "123456";
            _context.User?.Identity?.Name.Returns("testuser");
            _lobbyManager.IsHost("testuser", lobbyId).Returns(false);

            // Act
            var result = await _uut.UserIsHost(lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("User is not the host of the lobby"));
        }
        
        [Test]
        public async Task UserIsHost_UsernameIsNull_ReturnsError()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string?)null);

            // Act
            var result = await _uut.UserIsHost("123456");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        }
        
        [Test]
        public async Task GetUsersInLobby_ReturnsUsers()
        {
            // Arrange
            string lobbyId = "123456";
            var users = new List<ConnectedUserDTO> { new ConnectedUserDTO("testuser", "connection-id") };
            _context.User?.Identity?.Name.Returns("testuser");
            _lobbyManager.LobbyExists(lobbyId).Returns(true);
            _lobbyManager.GetUsersInLobby(lobbyId).Returns(users);

            // Act
            var result = await _uut.GetUsersInLobby(lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
            Assert.That(result.Value, Is.EqualTo(users));
        }
        
        [Test]
        public async Task GetUsersInLobby_LobbyDoesNotExist_ReturnsError()
        {
            // Arrange
            string lobbyId = "123456";
            _context.User?.Identity?.Name.Returns("testuser");
            _lobbyManager.LobbyExists(lobbyId).Returns(false);

            // Act
            var result = await _uut.GetUsersInLobby(lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        }
        
        [Test]
        public async Task GetUsersInLobby_UsernameIsNull_ReturnsError()
        {
            // Arrange
            _context.User?.Identity?.Name.Returns((string?)null);

            // Act
            var result = await _uut.GetUsersInLobby("123456");

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