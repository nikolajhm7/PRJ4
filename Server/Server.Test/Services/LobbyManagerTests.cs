using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;
using Server.API.Services;
using Server.API.Models;
using Server.API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Microsoft.Extensions.Azure;

namespace Server.Test.Services
{
    [TestFixture]
    public class LobbyManagerTests
    {
        private LobbyManager _uut;
        private IIdGenerator _idGenerator;
        private IGameRepository _gameRepository;

        [SetUp]
        public void Setup()
        {
            _idGenerator = Substitute.For<IIdGenerator>();
            _gameRepository = Substitute.For<IGameRepository>();

            _uut = new LobbyManager(_idGenerator, _gameRepository);
        }

        [Test]
        public void LobbyExists_LobbyExists_ReturnsTrue()
        {
            // Arrange
            string lobbyId = "123";
            _uut.lobbies.Add(lobbyId, new Lobby(lobbyId, "", 1, 10));

            // Act
            var result = _uut.LobbyExists(lobbyId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void LobbyExists_LobbyDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string lobbyId = "123";

            // Act
            var result = _uut.LobbyExists(lobbyId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsHost_IsHost_ReturnsTrue()
        {
            // Arrange
            string lobbyId = "123";
            string connectionId = "123";
            _uut.lobbies.Add(lobbyId, new Lobby(lobbyId, connectionId, 1, 10));

            // Act
            var result = _uut.IsHost(connectionId, lobbyId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsHost_IsNotHost_ReturnsFalse()
        {
            // Arrange
            string lobbyId = "123";
            string connectionId = "123";
            _uut.lobbies.Add(lobbyId, new Lobby(lobbyId, "456", 1, 10));

            // Act
            var result = _uut.IsHost(connectionId, lobbyId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetLobbyIdFromUser_UserInLobby_ReturnsLobbyId()
        {
            // Arrange
            string lobbyId = "123456";
            ConnectedUserDTO user = new ConnectedUserDTO("name", "123");
            var lobby = new Lobby(lobbyId, "123", 1, 10);
            lobby.Members.Add(user);
            _uut.lobbies.Add(lobbyId, lobby);
           

            // Act
            var result = _uut.GetLobbyIdFromUser(user);

            // Assert
            Assert.That(result, Is.EqualTo(lobbyId));
        }

        [Test]
        public void GetLobbyIdFromUser_UserNotInLobby_ReturnsNull()
        {
            // Arrange
            ConnectedUserDTO user = new ConnectedUserDTO("name", "123");

            // Act
            var result = _uut.GetLobbyIdFromUser(user);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetUsersInLobby_LobbyExists_ReturnsUsers()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user = new ConnectedUserDTO("name", "123");
            var lobby = new Lobby(lobbyId, "123", 1, 10);
            lobby.Members.Add(user);
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            var result = _uut.GetUsersInLobby(lobbyId);

            // Assert
            Assert.That(result, Is.EqualTo(new List<ConnectedUserDTO> { user }));
        }

        [Test]
        public void GetUsersInLobby_LobbyDoesNotExist_ReturnsEmptyList()
        {
            // Arrange
            string lobbyId = "123";

            // Act
            var result = _uut.GetUsersInLobby(lobbyId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateNewLobby_ReturnsLobbyId()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user = new ConnectedUserDTO("name", "123");
            _idGenerator.GenerateRandomLobbyId().Returns(lobbyId);
            //_gameRepository.GetMaxPlayers(1).Returns(10);

            // Act
            var result = await _uut.CreateNewLobby(user, 1);

            // Assert
            Assert.That(result, Is.EqualTo(lobbyId));
            Assert.That(_uut.lobbies[lobbyId].Members, Contains.Item(user));
        }

        [Test]
        public void AddToLobby_LobbyExists_ReturnsTrue()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user = new("name", "123");
            
            _uut.lobbies.Add(lobbyId, new Lobby(lobbyId, "123", 1, 10));
            
            // Act
            var result = _uut.AddToLobby(user, lobbyId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Msg, Is.Null);
            Assert.That(_uut.lobbies[lobbyId].Members, Contains.Item(user));
        }

        [Test]
        public void AddToLobby_LobbyDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user = new("name", "123");

            // Act
            var result = _uut.AddToLobby(user, lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Could not find lobby"));
        }

        [Test]
        public void AddToLobby_LobbyFull_ReturnsFalse()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user1 = new("name", "123");
            ConnectedUserDTO user2 = new("name1", "1234");
            var lobby = new Lobby(lobbyId, "123", 1, 1);
            lobby.Members.Add(user1);
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            var result = _uut.AddToLobby(user2, lobbyId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Msg, Is.EqualTo("Lobby is full"));
        }

        [Test]
        public void RemoveFromLobby_UserIsNotInLobbyAnymore()
        {
            // Arrange
            string lobbyId = "123";
            ConnectedUserDTO user = new("name", "123");
            var lobby = new Lobby(lobbyId, "123", 1, 1);
            lobby.Members.Add(user);
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            _uut.RemoveFromLobby(user, lobbyId);

            // Assert
            Assert.That(_uut.lobbies[lobbyId].Members, Does.Not.Contain(user));
        }

        [Test]
        public void RemoveLobby_LobbyIsRemoved()
        {
            // Arrange
            string lobbyId = "123";
            var lobby = new Lobby(lobbyId, "123", 1, 1);
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            _uut.RemoveLobby(lobbyId);

            // Assert
            Assert.That(_uut.lobbies, Does.Not.ContainKey(lobbyId));
        }

        [Test]
        public void StartGame_LobbyExists_StatusUpdated()
        {
            string lobbyId = "123";
            var lobby = new Lobby(lobbyId, "123", 1, 1);
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            _uut.StartGame(lobbyId);

            // Assert
            Assert.That(_uut.lobbies[lobbyId].Status, Is.EqualTo(GameStatus.InGame));
        }

        [Test]
        public void GetGameStatus_LobbyExists_ReturnsStatus()
        {
            string lobbyId = "123";
            var lobby = new Lobby(lobbyId, "123", 1, 1);
            var s = GameStatus.InGame;
            lobby.Status = s;
            _uut.lobbies.Add(lobbyId, lobby);

            // Act
            var result = _uut.GetGameStatus(lobbyId);

            // Assert
            Assert.That(result, Is.EqualTo(s));
        }

        [Test]
        public void GetGameStatus_LobbyDoesNotExist_ReturnNO_LOBBY()
        {
            string lobbyId = "123";

            // Act
            var result = _uut.GetGameStatus(lobbyId);

            // Assert
            Assert.That(result, Is.EqualTo(GameStatus.NO_LOBBY));
        }
    }
}
