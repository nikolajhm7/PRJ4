using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.DTO;
using Server.API.Games;
using Server.API.Models;
using Server.API.Repositories.Interfaces;
using Server.API.Services;
using Server.API.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
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
    
    [TearDown]
    public void TearDown()
    {
        _uut?.Dispose();
    }
}