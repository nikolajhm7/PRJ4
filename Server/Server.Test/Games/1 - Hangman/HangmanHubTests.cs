using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;
using Server.API.DTO;
using Server.API.Games;
using Server.API.Models;
using Server.API.Repositories.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Server.Test.Games;

[TestFixture]
public class HangmanHubTests
{
    private HangmanHub _uut;
    private IHubCallerClients _clients;
    private IGroupManager _groups;
    private HubCallerContext _context;

    private ILogger<HangmanHub> _logger;
    private IClientProxy _clientProxy;
    private ISingleClientProxy _singleClientProxy;
    private ILobbyManager _lobbyManager;
    private IRandomPicker _randomPicker;
    private ILogicManager<IHangmanLogic> _logicManager;
    private IHangmanLogic _logic;

    [SetUp]
    public void Setup()
    {
        _clients = Substitute.For<IHubCallerClients>();
        _groups = Substitute.For<IGroupManager>();
        _context = Substitute.For<HubCallerContext>();
        _clientProxy = Substitute.For<IClientProxy>();
        _singleClientProxy = Substitute.For<ISingleClientProxy>();
        _lobbyManager = Substitute.For<ILobbyManager>();
        _randomPicker = Substitute.For<IRandomPicker>();
        _logicManager = Substitute.For<ILogicManager<IHangmanLogic>>();
        _logic = Substitute.For<IHangmanLogic>();
        _logger = Substitute.For<ILogger<HangmanHub>>();

        _uut = new HangmanHub(_lobbyManager, _logicManager, _logger, _randomPicker)
        {
            Clients = _clients,
            Groups = _groups,
            Context = _context
        };
    }
    #region OnConnectedAsyncTests
    [Test]
    public async Task OnConnectedAsync_LobbyDoesNotExist_Disconnects()
    {
        // Arrange
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns((string?)null);
        
        // Act
        await _uut.OnConnectedAsync();
        
        // Assert
        _context.Received(1).Abort();
    }
    
    [Test]
    public async Task OnConnectedAsync_LobbyGameNotStarted_Disconnects()
    {
        // Arrange
        var lobbyId = "Id";
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns(lobbyId);
        _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InLobby);
        
        // Act
        await _uut.OnConnectedAsync();
        
        // Assert
        _context.Received(1).Abort();
    }
    
    [Test]
    public async Task OnConnectedAsync_LobbyExistsAndGameStarted_AddsToGroup()
    {
        // Arrange
        var lobbyId = "Id";
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns(lobbyId);
        _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InGame);

        // Act
        await _uut.OnConnectedAsync();
        
        // Assert
        await _groups.Received(1).AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>());
    }
    
    [Test]
    public async Task OnConnectedAsync_LobbyDoesNotExistAndGameStarted_AddsToGroup()
    {
        // Arrange
        var lobbyId = "Id";
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns(lobbyId);
        _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InGame);
        _logicManager.LobbyExists(lobbyId).Returns(true);
        _logicManager.TryGetValue(lobbyId, out var logic).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });

        // Act
        await _uut.OnConnectedAsync();
        
        // Assert
        await _groups.Received(1).AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>());
    }
    #endregion
    #region GuessLetterTests    
    [Test]
    public async Task GuessLetter_LobbyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";
        _logicManager.LobbyExists(lobbyId).Returns(false);

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');
        
        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("Lobby does not exist."));
    }
    
    [Test]
    public async Task GuessLetter_LobbyExists_ReturnsTrueAndSendsMessage()
    {
        // Arrange
        var lobbyId = "Id";
        var connection = "connection-id";
        var username = "Testuser";
        var user = new ConnectedUserDTO(username, connection);

        _context.User?.Identity?.Name.Returns(username);
        _context.ConnectionId.Returns(connection);

        var list = new List<ConnectedUserDTO> { user, new ConnectedUserDTO(username, connection) };
        _lobbyManager.GetUsersInLobby(lobbyId).Returns(list);

        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns("word");
        _logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });
        _logic.IsGameOver().Returns(false);

        // Mocking the queue
        var userQueue = new Queue<string>();
        userQueue.Enqueue(username); // Ensuring the currentUser is at the front
        _logic.GetQueue().Returns(userQueue);

        // Mocking the GuessLetter call
        _logic.GuessLetter(Arg.Any<char>(), out Arg.Any<List<int>>()).Returns(x =>
        {
            x[1] = new List<int> { 0 }; // Suppose 'c' is at position 0
            return true; // Letter guessed correctly
        });

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');
        
        // Assert
        await _clientProxy.Received(1).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        Assert.That(res.Success, Is.True);
        Assert.That(res.Msg, Is.Null);
    }
    
    [Test]
    public async Task GuessLetter_LobbyExistsGameWon_ReturnsTrueAnd2SendsMessage()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";
        var connection = "connection-id";
        var user = new ConnectedUserDTO(username, connection);

        _context.User?.Identity?.Name.Returns(username);
        _context.ConnectionId.Returns(connection);

        var list = new List<ConnectedUserDTO> { user, new ConnectedUserDTO(username, connection) };
        _lobbyManager.GetUsersInLobby(lobbyId).Returns(list);

        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });
        _logic.IsGameOver().Returns(true);

        // Mocking the queue
        var userQueue = new Queue<string>();
        userQueue.Enqueue(username); // Ensuring the currentUser is at the front
        _logic.GetQueue().Returns(userQueue);

        // Mocking the GuessLetter call
        _logic.GuessLetter(Arg.Any<char>(), out Arg.Any<List<int>>()).Returns(x =>
        {
            x[1] = new List<int> { 0 }; // Suppose 'c' is at position 0
            return true; // Letter guessed correctly
        });

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');
        
        // Assert
        await _clientProxy.Received(2).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        Assert.That(res.Success, Is.True);
        Assert.That(res.Msg, Is.Null);
    }

    [Test]
    public async Task GuessLetter_UserNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";

        // Simulate an unauthenticated user
        _context.User?.Identity?.Name.Returns((string?)null);
        //_logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        //{
        //    x[1] = _logic;
        //    return true;
        //});

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');

        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("User is not authenticated."));
    }

    [Test]
    public async Task GuessLetter_UserQueueIsNull_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns(username);
        _logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });

        // Mocking the queue to be null
        _logic.GetQueue().Returns((Queue<string>?)null);

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');

        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("There is no queue for the game"));
    }

    [Test]
    public async Task GuessLetter_CurrentUserNotAtFrontOfQueue_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";
        var currentUser = "Currentuser";
        var otherUser = "Otheruser";

        _context.User?.Identity?.Name.Returns(currentUser);
        _logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });

        // Mocking the queue with another user at the front
        var userQueue = new Queue<string>();
        userQueue.Enqueue(otherUser);
        userQueue.Enqueue(currentUser);
        _logic.GetQueue().Returns(userQueue);

        // Act
        var res = await _uut.GuessLetter(lobbyId, 'c');

        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("Not the users turn!"));
    }
    #endregion
    #region RestartGameTests
    [Test]
    public async Task RestartGame_LobbyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";

        // Act
        var res = await _uut.RestartGame(lobbyId);
        
        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("Lobby does not exist."));
    }
    
    [Test]
    public async Task RestartGame_LobbyExists_ReturnsTrueAndSendsMessage()
    {
        // Arrange
        var lobbyId = "Id";
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _logicManager.TryGetValue(Arg.Any<string>(), out Arg.Any<IHangmanLogic>()).Returns(x =>
        {
            x[1] = _logic;
            return true;
        });

        // Act
        var res = await _uut.RestartGame(lobbyId);
        
        // Assert
        await _clientProxy.Received(1).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        Assert.That(res.Success, Is.True);
        Assert.That(res.Msg, Is.Null);
    }
    #endregion
    #region OnDisconnectedAsyncTests
    [Test]
    public async Task OnDisconnectedAsync_CantFindLobby_SendNoMessage()
    {
        // Arrange
        var lobbyId = "Id";
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns((string?)null);

        // Act
        await _uut.OnDisconnectedAsync((Exception?)null);
        
        // Assert
        await _groups.DidNotReceive().RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        await _clientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        _lobbyManager.DidNotReceive().RemoveLobby(Arg.Any<string>());
        _lobbyManager.DidNotReceive().RemoveFromLobby(Arg.Any<ConnectedUserDTO>(), Arg.Any<string>());
    }
    
    [Test]
    public async Task OnDisconnectedAsync_CanFindLobbyIsNotHost_RemoveAndSendMessage()
    {
        // Arrange
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns("test");
        _lobbyManager.IsHost(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        await _uut.OnDisconnectedAsync((Exception?)null);
        
        // Assert
        await _groups.Received(1).RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        await _clientProxy.Received(1).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        _lobbyManager.Received(1).RemoveFromLobby(Arg.Any<ConnectedUserDTO>(), Arg.Any<string>());
    }
    
    [Test]
    public async Task OnDisconnectedAsync_CanFindLobbyIsHost_CloseAndSendMessage()
    {
        // Arrange
        var user = new ConnectedUserDTO("", "");
        var list = new List<ConnectedUserDTO> { user }; 
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _lobbyManager.GetLobbyIdFromUsername(Arg.Any<string>()).Returns("test");
        _lobbyManager.IsHost(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _lobbyManager.GetUsersInLobby(Arg.Any<string>()).Returns(list);

        // Act
        await _uut.OnDisconnectedAsync((Exception?)null);
        
        // Assert
        await _groups.Received(1).RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        await _clientProxy.Received(1).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        _lobbyManager.Received(1).RemoveLobby(Arg.Any<string>());
        _lobbyManager.DidNotReceive().RemoveFromLobby(Arg.Any<ConnectedUserDTO>(), Arg.Any<string>());
    }
    #endregion
    #region GetUsersInGameTests
    [Test]
    public async Task GetUsersInGame_UserNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";

        // Simulate an unauthenticated user
        _context.User?.Identity?.Name.Returns((string?)null);

        // Act
        var res = await _uut.GetUsersInGame(lobbyId);

        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("Authentication context is not available."));
        _logger.Received(1).Log(
            LogLevel.Warning,
            0,
            Arg.Is<object>(v => v.ToString() == "Context.User or Context.User.Identity is null."),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task GetUsersInGame_LobbyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns(username);
        _lobbyManager.LobbyExists(lobbyId).Returns(false);

        // Act
        var res = await _uut.GetUsersInGame(lobbyId);

        // Assert
        Assert.That(res.Success, Is.False);
        Assert.That(res.Msg, Is.EqualTo("Lobby does not exist."));
        _logger.Received(1).Log(
            LogLevel.Error,
            0,
            Arg.Is<object>(v => v.ToString() == $"Attempt to get users in non-existing lobby {lobbyId}."),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task GetUsersInGame_LobbyExists_ReturnsTrue()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns(username);
        var users = new List<ConnectedUserDTO>
        {
            new ConnectedUserDTO("User1", "Connection1"),
            new ConnectedUserDTO("User2", "Connection2")
        };
        _lobbyManager.LobbyExists(lobbyId).Returns(true);
        _lobbyManager.GetUsersInLobby(lobbyId).Returns(users);

        // Act
        var res = await _uut.GetUsersInGame(lobbyId);

        // Assert
        Assert.That(res.Success, Is.True);
        Assert.That(res.Msg, Is.Null);
        _logger.Received(1).Log(
            LogLevel.Information,
            0,
            Arg.Is<object>(v => v.ToString() == $"{username} successfully got users in game {lobbyId}."),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }
    #endregion
    #region InitQueueForGameTests
    [Test]
    public async Task InitQueueForGame_UsersExistInLobby_InitializesQueue()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns(username);

        // Properly initialize the list of ConnectedUserDTO objects
        var users = new List<ConnectedUserDTO>
        {
            new ConnectedUserDTO("User1", "Connection1"),
            new ConnectedUserDTO("User2", "Connection2"),
            new ConnectedUserDTO(username, "Connection3") // Current user
        };

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });
        _lobbyManager.GetUsersInLobby(lobbyId).Returns(users);


        // Act
        _uut.InitQueueForGame(lobbyId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            0,
            Arg.Is<object>(v => v.ToString() == $"{username} successfully got user queue in game {lobbyId}."),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task InitQueueForGame_ValidLobbyId_QueueInitialized()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var connectionId = "ConnectionId";

        var users = new List<ConnectedUserDTO>
            {
                new ConnectedUserDTO(username, connectionId)
            };

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        _lobbyManager.GetUsersInLobby(lobbyId).Returns(users);

        // Act
        await _uut.InitQueueForGame(lobbyId);

        // Assert
        var userQueue = logic.GetQueue();
        Assert.That(1, Is.EqualTo(userQueue.Count));
        Assert.That(username, Is.EqualTo(userQueue.Peek()));
        _logger.Received(1).Log(
            LogLevel.Information,
            0,
            Arg.Is<object>(v => v.ToString().Contains("successfully got user queue in game")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task InitQueueForGame_NoUsersInLobby_QueueNotInitialized()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns(username);

        // Return an empty list of users
        var users = new List<ConnectedUserDTO>();

        _lobbyManager.GetUsersInLobby(lobbyId).Returns(users);

        // Act
        await _uut.InitQueueForGame(lobbyId);

        // Assert
        _logic.DidNotReceive().SetQueue(Arg.Any<Queue<string>>());
        _logger.Received(1).Log(
            LogLevel.Error,
            0,
            Arg.Is<object>(v => v.ToString().Contains("Attempt to get user queue in non-existing lobby Id")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task InitQueueForGame_UserNotAuthenticated_QueueNotInitialized()
    {
        // Arrange
        var lobbyId = "Id";
        var username = "Testuser";

        _context.User?.Identity?.Name.Returns((string)null); // Unauthenticated User

        // Act
        await _uut.InitQueueForGame(lobbyId);

        // Assert
        _logic.DidNotReceive().SetQueue(Arg.Any<Queue<string>>());
        _logger.Received(1).LogWarning("Context.User or Context.User.Identity is null.");
    }
#endregion
    #region GetFrontPlayerForGameTests
    [Test]
    public async Task GetFrontPlayerForGame_ValidLobbyAndUser_QueueNotEmpty()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var connectionId = "ConnectionId";

        var userQueue = new Queue<string>();
        userQueue.Enqueue(username);

        var logic = new HangmanLogic(_randomPicker);
        logic.SetQueue(userQueue);

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        _context.User?.Identity?.Name.Returns(username);

        // Act
        var result = await _uut.GetFrontPlayerForGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(username));
        _logger.Received(1).Log(
            LogLevel.Information,
            0,
            Arg.Is<object>(v => v.ToString().Contains("successfully got user queue in game")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
        _logger.Received(1).Log(
            LogLevel.Debug,
            0,
            Arg.Is<object>(v => v.ToString().Contains("Attempting to get user queue for game with LobbyId")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task GetFrontPlayerForGame_LobbyDoesNotExist()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        _logicManager.TryGetValue(lobbyId, out _).Returns(false);

        _context.User?.Identity?.Name.Returns(username);

        // Act
        var result = await _uut.GetFrontPlayerForGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
        _logger.Received(1).Log(
            LogLevel.Error,
            0,
            Arg.Is<object>(v => v.ToString().Contains("Attempt to get user queue in non-existing lobby")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task GetFrontPlayerForGame_QueueIsEmpty()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        var logic = new HangmanLogic(_randomPicker);
        logic.SetQueue(new Queue<string>());

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        _context.User?.Identity?.Name.Returns(username);

        // Act
        var result = await _uut.GetFrontPlayerForGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("There is no queue for the game"));
    }

    [Test]
    public async Task GetFrontPlayerForGame_UserNotAuthenticated()
    {
        // Arrange
        var lobbyId = "lobby1";

        _context.User?.Identity?.Name.Returns((string)null);

        // Act
        var result = await _uut.GetFrontPlayerForGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));
        _logger.Received(1).Log(
            LogLevel.Warning,
            0,
            Arg.Is<object>(v => v.ToString().Contains("Context.User or Context.User.Identity is null.")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }
#endregion
    #region LeaveGameTests
    [Test]
    public async Task LeaveGame_UserSuccessfullyLeavesGame()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var connectionId = "ConnectionId";

        _context.User.Identity.Name.Returns(username);
        _context.ConnectionId.Returns(connectionId);
        _lobbyManager.LobbyExists(lobbyId).Returns(true);

        // Act
        var result = await _uut.LeaveGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Msg, Is.Null);

        _lobbyManager.Received(1).RemoveFromLobby(Arg.Is<ConnectedUserDTO>(u => u.Username == username && u.ConnectionId == connectionId), lobbyId);
        _logger.Received(1).Log(
            LogLevel.Debug,
            0,
            Arg.Is<object>(v => v.ToString().Contains($"Attempting to leave lobby {lobbyId} by user {username}")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task LeaveGame_UserNotAuthenticated()
    {
        // Arrange
        var lobbyId = "lobby1";

        _context.User.Identity.Name.Returns((string)null);

        // Act
        var result = await _uut.LeaveGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Authentication context is not available."));

        _logger.Received(1).Log(
            LogLevel.Warning,
            0,
            Arg.Is<object>(v => v.ToString().Contains("Context.User or Context.User.Identity is null.")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public async Task LeaveGame_LobbyDoesNotExist()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        _context.User.Identity.Name.Returns(username);
        _lobbyManager.LobbyExists(lobbyId).Returns(false);

        // Act
        var result = await _uut.LeaveGame(lobbyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));

        _logger.Received(1).Log(
            LogLevel.Error,
            0,
            Arg.Is<object>(v => v.ToString().Contains($"Attempt to leave non-existing lobby {lobbyId}")),
            null,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }
    #endregion
    #region RemovePlayerFromQueueTests
    [Test]
    public async Task RemovePlayerFromQueue_PlayerSuccessfullyRemoved()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var otherUser = "OtherUser";

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        var queue = new Queue<string>(new[] { username, otherUser });
        logic.SetQueue(queue);

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        // Act
        var result = await _uut.RemovePlayerFromQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Msg, Is.Null);

        var updatedQueue = logic.GetQueue();
        Assert.That(updatedQueue.Contains(username), Is.False);
        Assert.That(updatedQueue.Contains(otherUser), Is.True);
    }

    [Test]
    public async Task RemovePlayerFromQueue_PlayerNotFoundInQueue()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var otherUser = "OtherUser";

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        var queue = new Queue<string>(new[] { otherUser });
        logic.SetQueue(queue);

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        // Act
        var result = await _uut.RemovePlayerFromQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Player not found in the queue."));
    }

    [Test]
    public async Task RemovePlayerFromQueue_LobbyDoesNotExist()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        _logicManager.TryGetValue(lobbyId, out _).Returns(false);

        // Act
        var result = await _uut.RemovePlayerFromQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
    }
    #endregion
    #region AddPlayerToQueueTests
    [Test]
    public async Task AddPlayerToQueue_PlayerSuccessfullyAdded()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";
        var otherUser = "OtherUser";

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        var queue = new Queue<string>(new[] { otherUser });
        logic.SetQueue(queue);

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        // Act
        var result = await _uut.AddPlayerToQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Msg, Is.Null);

        var updatedQueue = logic.GetQueue();
        Assert.That(updatedQueue.Contains(username), Is.True);
    }

    [Test]
    public async Task AddPlayerToQueue_PlayerAlreadyInQueue()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        var logic = new HangmanLogic(Substitute.For<IRandomPicker>());
        var queue = new Queue<string>(new[] { username });
        logic.SetQueue(queue);

        _logicManager.TryGetValue(lobbyId, out _).Returns(x => {
            x[1] = logic;
            return true;
        });

        // Act
        var result = await _uut.AddPlayerToQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Player is already in the queue."));
    }

    [Test]
    public async Task AddPlayerToQueue_LobbyDoesNotExist()
    {
        // Arrange
        var lobbyId = "lobby1";
        var username = "TestUser";

        _logicManager.TryGetValue(lobbyId, out _).Returns(false);

        // Act
        var result = await _uut.AddPlayerToQueue(lobbyId, username);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Msg, Is.EqualTo("Lobby does not exist."));
    }
    #endregion

    [TearDown]
    public void TearDown()
    {
        _uut?.Dispose();
    }
}