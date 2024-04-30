using Server.API.Services;
using NSubstitute;

namespace Server.Test.Services
{


    [TestFixture]
    public class RandomGeneratorTests
    {
        private RandomGenerator _uut;

        [SetUp]
        public void Setup()
        {
            _uut = new RandomGenerator();
        }

        [Test]
        public void GenerateRandomLobbyID_ReturnsSixDigitString()
        {
            var id = _uut.GenerateRandomLobbyId();

            Assert.That(id, Has.Length.EqualTo(6), "The generated ID should be 6 characters long.");
        }

        [Test]
        public void GenerateRandomLobbyID_ReturnsOnlyDigits()
        {
            var id = _uut.GenerateRandomLobbyId();

            Assert.That(id, Does.Match(@"^\d{6}$"), "The generated ID should consist only of digits.");
        }

        [Test]
        public void GenerateRandomLobbyID_ReturnsNumberWithinRange()
        {
            var id = _uut.GenerateRandomLobbyId();
            var idNumber = int.Parse(id);

            Assert.That(idNumber, Is.InRange(0, 999999), "The generated number should be between 0 and 999999.");
        }

        [Test]
        public void PickRandomItem_ReturnsItemFromList()
        {
            // Arrange
            var list = new List<string> { "string1", "string2", "string3", "string4" };
            
            // Act
            var item = _uut.PickRandomItem(list);
            
            // Assert
            Assert.That(list, Does.Contain(item));
        }
    }

}
