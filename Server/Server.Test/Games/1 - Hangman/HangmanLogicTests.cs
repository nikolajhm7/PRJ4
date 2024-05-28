using NSubstitute;
using Server.API.Games;
using Server.API.Services.Interfaces;

namespace Server.Test.Games;

[TestFixture]
public class HangmanLogicTests
{
    private HangmanLogic _uut;
    private IRandomPicker _randomPicker;

    [SetUp]
    public void Setup()
    {
        _randomPicker = Substitute.For<IRandomPicker>();

        _uut = new(_randomPicker);
    }

    [Test]
    public void StartGame_SetsSecretWord()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);

        // Act
        _uut.StartGame();

        // Assert
        Assert.That(_uut.SecretWord, Is.EqualTo(word));
    }

    [Test]
    public void GuessLetter_CorrectGuess_ReturnsTrue()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var res = _uut.GuessLetter('t', out var pos);

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void GuessLetter_WrongGuess_ReturnsFalseAndEmptyArray()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var res = _uut.GuessLetter('a', out var pos);

        // Assert
        Assert.That(res, Is.False);
        Assert.That(pos, Is.Empty);
    }

    [Test]
    public void GuessLetter_Lowercase_IsCorrect()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var res = _uut.GuessLetter('t', out var pos);

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void GuessLetter_Uppercase_IsCorrect()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var res = _uut.GuessLetter('T', out var pos);

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void GuessLetter_LettersInCorrectPosition()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        _uut.GuessLetter('t', out var pos1);
        _uut.GuessLetter('e', out var pos2);
        _uut.GuessLetter('s', out var pos3);

        // Assert
        Assert.That(pos1, Is.EqualTo(new List<int> { 0, 3 }));
        Assert.That(pos2, Is.EqualTo(new List<int> { 1 }));
        Assert.That(pos3, Is.EqualTo(new List<int> { 2 })); 
    }

    [Test]
    public void GuessLetter_NotALetter_ReturnsFalse()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var res = _uut.GuessLetter('1', out var pos);

        // Assert
        Assert.That(res, Is.False);
        Assert.That(pos, Is.Empty);
    }

    [Test]
    public void GuessLetter_AlreadyGuessed_ReturnsFalse()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        _uut.GuessLetter('t', out var pos1);
        var res = _uut.GuessLetter('t', out var pos);

        // Assert
        Assert.That(res, Is.False);
        Assert.That(pos, Is.Empty);
    }

    [Test]
    public void IsGameOver_UserWon_ReturnsTrue()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('t', out var pos1);
        _uut.GuessLetter('e', out var pos2);
        _uut.GuessLetter('s', out var pos3);

        // Act
        var res = _uut.IsGameOver();

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void IsGameOver_UserLost_ReturnsTrue()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('a', out var pos1);
        _uut.GuessLetter('b', out var pos2);
        _uut.GuessLetter('c', out var pos3);
        _uut.GuessLetter('d', out var pos4);
        _uut.GuessLetter('f', out var pos5);
        _uut.GuessLetter('g', out var pos6);

        // Act
        var res = _uut.IsGameOver();

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void IsGameOver_ReturnsFalse()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);

        _uut.StartGame();
        _uut.GuessLetter('t', out var pos1);

        // Act
        var res = _uut.IsGameOver();

        // Assert
        Assert.That(res, Is.False);
    }

    [Test]
    public void DidUserWin_UserWon_ReturnsTrue()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('t', out var pos1);
        _uut.GuessLetter('e', out var pos2);
        _uut.GuessLetter('s', out var pos3);

        // Act
        var res = _uut.DidUserWin();

        // Assert
        Assert.That(res, Is.True);
    }

    [Test]
    public void DidUserWin_UserLost_ReturnsFalse()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        for(int i = 0; i < _uut.MaxIncorrectGuesses; i++)
        {
            _uut.GuessLetter('a', out var pos1);    
        }

        // Act
        var res = _uut.DidUserWin();

        // Assert
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void DidUserWin_GuessesEqualMaxGuesses_ReturnsFalse()
    {
        // Arrange
        string word = "zzzzz";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        for(int i = 0; i < _uut.MaxIncorrectGuesses; i++)
        {
            char letter = (char)('a' + i);
            _uut.GuessLetter(letter, out var pos1);    
        }

        // Act
        var res = _uut.DidUserWin();

        // Assert
        Assert.That(res, Is.False);
    }
    [Test]
    public void GetGuessedLetters_NoLettersGuessed_ReturnsEmptyList()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        // Act
        var guessedLetters = _uut.GetGuessedLetters();

        // Assert
        Assert.That(guessedLetters, Is.Empty);
    }

    [Test]
    public void GetGuessedLetters_SomeLettersGuessedCorrectly_ReturnsGuessedLetters()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('t', out _);
        _uut.GuessLetter('e', out _);

        // Act
        var guessedLetters = _uut.GetGuessedLetters();

        // Assert
        Assert.That(guessedLetters, Is.EquivalentTo(new List<char> { 't', 'e' }));
    }

    [Test]
    public void GetGuessedLetters_SomeLettersGuessedIncorrectly_ReturnsGuessedLetters()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('a', out _);
        _uut.GuessLetter('b', out _);

        // Act
        var guessedLetters = _uut.GetGuessedLetters();

        // Assert
        Assert.That(guessedLetters, Is.EquivalentTo(new List<char> { 'a', 'b' }));
    }

    [Test]
    public void GetGuessedLetters_MixedGuesses_ReturnsAllGuessedLetters()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('t', out _);
        _uut.GuessLetter('a', out _);
        _uut.GuessLetter('e', out _);
        _uut.GuessLetter('b', out _);

        // Act
        var guessedLetters = _uut.GetGuessedLetters();

        // Assert
        Assert.That(guessedLetters, Is.EquivalentTo(new List<char> { 't', 'a', 'e', 'b' }));
    }

    [Test]
    public void GetGuessedLetters_RepeatedGuesses_ReturnsUniqueGuessedLetters()
    {
        // Arrange
        string word = "test";
        _randomPicker.PickRandomItem(Arg.Any<List<string>>()).Returns(word);
        _uut.StartGame();

        _uut.GuessLetter('t', out _);
        _uut.GuessLetter('t', out _); // Repeated guess
        _uut.GuessLetter('e', out _);
        _uut.GuessLetter('e', out _); // Repeated guess

        // Act
        var guessedLetters = _uut.GetGuessedLetters();

        // Assert
        Assert.That(guessedLetters, Is.EquivalentTo(new List<char> { 't', 'e' }));
    }
}