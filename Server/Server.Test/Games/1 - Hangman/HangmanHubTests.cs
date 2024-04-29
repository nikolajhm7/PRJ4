using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.DTO;
using Server.API.Games;
using Server.API.Models;
using Server.API.Repositories.Interfaces;
using Server.API.Services.Interfaces;

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

        _logger = Substitute.For<ILogger<HangmanHub>>();

        _uut = new HangmanHub(_lobbyManager, _logger, _randomPicker)
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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns((string?)null);
        
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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns(lobbyId);
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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns(lobbyId);
        _lobbyManager.GetGameStatus(lobbyId).Returns(GameStatus.InGame);

        // Act
        await _uut.OnConnectedAsync();
        
        // Assert
        await _groups.Received(1).AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>());
    }
    
    [Test]
    public async Task StartGame_StartsGame_ReturnsTrueSendsMessage()
    {
        // Arrange
        var lobbyId = "Id";
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);

        // Act
        var res = await _uut.StartGame(lobbyId);
        
        // Assert
        await _clientProxy.Received(1).SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
        Assert.That(res.Success, Is.True);
    }
    
    [Test]
    public async Task StartGame_AlreadyExists_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";
        await _uut.StartGame(lobbyId);

        // Act
        var res = await _uut.StartGame(lobbyId);
        
        // Assert
        Assert.That(res.Success, Is.False);
    }
    
    [Test]
    public async Task GuessLetter_LobbyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var lobbyId = "Id";

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
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns("word");
        await _uut.StartGame(lobbyId);
        _clientProxy.ClearReceivedCalls();

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
        _clients.Group(Arg.Any<string>()).Returns(_clientProxy);
        await _uut.StartGame(lobbyId);
        _clientProxy.ClearReceivedCalls();

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
        await _uut.StartGame(lobbyId);
        
        _clientProxy.ClearReceivedCalls();

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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns((string?)null);

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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns("test");
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
        _lobbyManager.GetLobbyIdFromUser(Arg.Any<ConnectedUserDTO>()).Returns("test");
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