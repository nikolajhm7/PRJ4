using Server.API.Services;

namespace Server.Test.Services;

[TestFixture]
public class LogicManagerTests
{
    private LogicManager<int> _uut;

    [SetUp]
    public void Setup()
    {
        _uut = new LogicManager<int>();
    }

    [Test]
    public void Add_AddsItem()
    {
        // Arrange
        var lobbyId = "123";
        
        // Act
        _uut.Add(lobbyId, 1);
        
        // Assert
        Assert.That(_uut.LobbyLogic.ContainsKey(lobbyId), Is.True);
    }

    [Test]
    public void LobbyExists_Exists_ReturnTrue()
    {
        // Arrange
        var lobbyId = "123";
        _uut.Add(lobbyId, 1);
        
        // Act
        var res = _uut.LobbyExists(lobbyId);
        
        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void TryGetValue_Exists_ReturnTrueAndValue()
    {
        // Arrange
        var lobbyId = "123";
        int logic = 1;
        _uut.Add(lobbyId, logic);
        
        // Act
        var res = _uut.TryGetValue(lobbyId, out var outLogic);
        
        // Assert
        Assert.That(logic, Is.EqualTo(outLogic));
        Assert.That(res, Is.True);
    }

    [Test]
    public void Remove_RemovesItem()
    {
        // Arrange
        var lobbyId = "123";
        _uut.Add(lobbyId, 1);
        
        // Act
        _uut.Remove(lobbyId);
        
        // Assert
        Assert.That(!_uut.LobbyLogic.ContainsKey(lobbyId));
    }
}