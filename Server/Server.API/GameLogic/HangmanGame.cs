using Microsoft.AspNetCore.SignalR;
using Server.API.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Server.API.GameLogic
{
    public class HangmanGame
    {
        private static readonly Random rand = new Random();
        private readonly Dictionary<string, List<string>> wordCategories;
        private string secretWord;
        private readonly List<char> guessedLetters = new List<char>();
        private readonly IHubContext<HangmanHub> hubContext;

        // Constructor with additional parameter IHubContext<HangmanHub>
        public HangmanGame(Dictionary<string, List<string>> categories, IHubContext<HangmanHub> hubContext)
        {
            wordCategories = categories;
            this.hubContext = hubContext;
        }

        // Method to start the game
        public async Task StartGame(string category, string connectionId)
        {
            // Check if the chosen category is valid
            if (!wordCategories.ContainsKey(category))
            {
                // Send invalid category message to the client
                await hubContext.Clients.Client(connectionId).SendAsync("InvalidCategory");
                return;
            }

            // Select a random word from the chosen category
            SelectRandomWord(category);
            await hubContext.Clients.All.SendAsync("GameStarted", GetGuessedWord().Length);
        }

        // Method to handle letter guesses
        public async Task GuessLetter(char letter)
        {
            // Convert the letter to uppercase
            letter = char.ToUpper(letter);
            bool isCorrect = GuessLetterLogic(letter);

            // Send guess result to all clients
            await hubContext.Clients.All.SendAsync("GuessResult", letter, isCorrect);

            // Check if the game is over
            bool isGameOver = IsGameOver(out bool isWin);
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
            }
        }

        // Select a random word from the chosen category
        private void SelectRandomWord(string category)
        {
            List<string> wordsInCategory = wordCategories[category];
            secretWord = wordsInCategory[rand.Next(wordsInCategory.Count)].ToUpper();
        }

        // Logic to handle letter guesses
        private bool GuessLetterLogic(char letter)
        {
            if (!char.IsLetter(letter) || guessedLetters.Contains(letter))
            {
                return false;
            }

            guessedLetters.Add(letter);
            return secretWord.Contains(letter);
        }

        // Check if the game is over
        private bool IsGameOver(out bool isWin)
        {
            isWin = !secretWord.Any(c => !guessedLetters.Contains(c));
            return isWin || GetIncorrectGuessCount() >= 6;
        }

        // Gets the guessed word with underscores for missing letters
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

        // Gets the number of incorrect guesses
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