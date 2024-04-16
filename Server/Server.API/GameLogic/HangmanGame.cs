using Microsoft.AspNetCore.SignalR;
using Server.API.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.API.GameLogic
{
    public class HangmanGame : IGame
    {
        private static readonly Random rand = new Random();
        private readonly Dictionary<string, List<string>> wordCategories;
        private string secretWord;
        private readonly List<char> guessedLetters = new List<char>();
        private readonly IHubContext<HangmanHub> hubContext;

        public event EventHandler<GameStartedEventArgs> GameStarted;
        public event EventHandler<GameEndedEventArgs> GameEnded;

        // Constructor with HangmanGame parameters and IHubContext
        public HangmanGame(Dictionary<string, List<string>> categories, IHubContext<HangmanHub> hubContext)
        {
            wordCategories = categories;
            this.hubContext = hubContext;
        }

        // Method to start a new game with the specified parameters
        public async Task StartGame(GameParameters parameters)
        {
            if (!wordCategories.ContainsKey(parameters.WordToGuess))
            {
                await hubContext.Clients.Client(parameters.Players.First()).SendAsync("InvalidCategory");
                return;
            }

            SelectRandomWord(parameters.WordToGuess);
            await hubContext.Clients.All.SendAsync("GameStarted", GetGuessedWord().Length);

            GameStarted?.Invoke(this, new GameStartedEventArgs(parameters));
        }

        // Method to make a move or perform an action in the game - Guess a letter
        public async Task MakeMove(GameMove move)
        {
            char letter = char.ToUpper(move.GuessLetter);
            bool isCorrect = GuessLetterLogic(letter);

            await hubContext.Clients.All.SendAsync("GuessResult", letter, isCorrect);

            bool isGameOver = IsGameOver(out bool isWin, new GameParameters());
            if (isGameOver)
            {
                if (isWin)
                {
                    await hubContext.Clients.All.SendAsync("GameOver", true, secretWord);
                }
                else
                {
                    await hubContext.Clients.All.SendAsync("GameOver", false, secretWord);
                }

                GameEnded?.Invoke(this, new GameEndedEventArgs(isWin ? GameResult.Win : GameResult.Loss));
            }
        }

        // Method to select a random word from the specified category
        private void SelectRandomWord(string category)
        {
            List<string> wordsInCategory = wordCategories[category];
            secretWord = wordsInCategory[rand.Next(wordsInCategory.Count)].ToUpper();
        }

        // Method to process the logic of guessing a letter
        private bool GuessLetterLogic(char letter)
        {
            if (!char.IsLetter(letter) || guessedLetters.Contains(letter))
            {
                return false;
            }

            guessedLetters.Add(letter);
            return secretWord.Contains(letter);
        }

        private bool IsGameOver(out bool isWin, GameParameters maxIncorrectGuess)
        {
            isWin = !secretWord.Any(c => !guessedLetters.Contains(c));
            return isWin || GetIncorrectGuessCount() >= maxIncorrectGuess.MaxIncorrectGuesses; 

        }

        private string GetGuessedWord()
        {
            string guessedWord = "";
            foreach (char letter in secretWord)
            {
                if (guessedLetters.Contains(letter))
                {
                    guessedWord += letter;
                }
                else
                {
                    guessedWord += "_";
                }
            }
            return guessedWord;
        }

        private int GetIncorrectGuessCount()
        {
            int count = 0;
            foreach (char letter in guessedLetters)
            {
                if (!secretWord.Contains(letter))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
