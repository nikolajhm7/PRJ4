using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Server.API.Models;
using Server.API.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace Server.API.Games
{
    public class HangmanLogic(IRandomPicker picker) : IHangmanLogic
    {
        public readonly int MaxIncorrectGuesses = 6;
        private string _secretWord = "";
        public string SecretWord { get { return _secretWord; } }
        private int _currentGuessCount = 0;
        private List<char> _guessedLetters = [];
        private readonly IRandomPicker _picker = picker;
        private Queue<string> _userQueue = [];

        public int StartGame()
        {
            _currentGuessCount = 0;
            _guessedLetters = [];

            SelectRandomWord();
            return _secretWord.Length;
        }

        public bool GuessLetter(char letter, out List<int> positions)
        {

            letter = char.ToLower(letter);
            if (!char.IsLetter(letter) || _guessedLetters.Contains(letter))
            {
                positions = [];
                return false;
            }

            if (!SecretWord.Contains(letter)) 
            {
                _currentGuessCount++; 
            }

            _guessedLetters.Add(letter);

            positions = FindLetterPositions(letter);
            return SecretWord.Contains(letter);
        }
        
        public bool IsGameOver()
        {
            var isWin = !SecretWord.Any(c => !_guessedLetters.Contains(c));

            return isWin || _currentGuessCount >= MaxIncorrectGuesses;
        }

        public bool DidUserWin()
        {
            if (_currentGuessCount < MaxIncorrectGuesses)
                return SecretWord.All(c => _guessedLetters.Contains(c));
            return false;
        }

        private void SelectRandomWord()
        {
            // Dette skal kun bruges så længe kategorien skal være random:
            var values = Words.CategoryMap.Values.ToList();
            var words = _picker.PickRandomItem(values);
            //

            //List<string> wordsInCategory = wordCategories[category];
            _secretWord = _picker.PickRandomItem(words);
        }

        private List<int> FindLetterPositions(char letter)
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < SecretWord.Length; i++)
            {
                if (SecretWord[i] == letter)
                {
                    positions.Add(i);
                }
            }
            return positions;
        }

        public List<char> GetGuessedLetters()
        {
            return _guessedLetters;
        }

        public void SetQueue(Queue<string> userQueue)
        {
            _userQueue = userQueue;
        }

        public Queue<string> GetQueue()
        {
            return _userQueue;
        }
    }
}
