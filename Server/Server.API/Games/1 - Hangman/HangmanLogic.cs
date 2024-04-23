using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Server.API.Games
{
    public class HangmanLogic(string lobbyId)
    {
        public string LobbyId { get; set; } = lobbyId;
        private readonly int _maxIncorrectGuesses = 5;
        private string _secretWord = "";
        private int _currentGuessCount = 0;
        private List<char> _guessedLetters = [];
        private readonly Random _rand = new();

        public string StartGame()
        {
            SelectRandomWord();
            return _secretWord;
        }

        public bool GuessLetter(char letter)
        {
            letter = char.ToLower(letter);
            if (!char.IsLetter(letter) || _guessedLetters.Contains(letter)) return false;

            _guessedLetters.Add(letter);
            return _secretWord.Contains(letter);
        }

        public string RestartGame()
        {
            _currentGuessCount = 0;
            _guessedLetters = [];

            SelectRandomWord();
            return _secretWord;
        }

        private void SelectRandomWord()
        {
            // Dette skal kun bruges så længe kategorien skal være random:
            List<List<string>> values = Words.CategoryMap.Values.ToList();
            List<string> wordsInCategory = values[_rand.Next(values.Count)];
            //

            //List<string> wordsInCategory = wordCategories[category];
            _secretWord = wordsInCategory[_rand.Next(wordsInCategory.Count)].ToUpper();
        }
        
        public bool IsGameOver()
        {
            var isWin = !_secretWord.Any(c => !_guessedLetters.Contains(c));
            return isWin || _currentGuessCount >= _maxIncorrectGuesses;
        }
    }
}
