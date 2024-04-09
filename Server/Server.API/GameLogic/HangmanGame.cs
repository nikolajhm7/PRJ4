//using Microsoft.AspNetCore.SignalR;
//using Server.API.Hubs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Server.API.GameLogic
//{
//    public class HangmanGame : IGame
//    {
//        private static readonly Random rand = new Random();
//        private readonly Dictionary<string, List<string>> wordCategories;
//        private string secretWord;
//        private readonly List<char> guessedLetters = new List<char>();
//        private readonly IHubContext<HangmanHub> hubContext;

//        public event EventHandler<GameStartedEventArgs> GameStarted;
//        public event EventHandler<GameEndedEventArgs> GameEnded;

//        public HangmanGame(Dictionary<string, List<string>> categories, IHubContext<HangmanHub> hubContext)
//        {
//            wordCategories = categories;
//            this.hubContext = hubContext;
//        }

//        public async Task StartGame(GameParameters parameters)
//        {
//            if (!wordCategories.ContainsKey(parameters.WordToGuess))
//            {
//                await hubContext.Clients.Client(parameters.Players.First()).SendAsync("InvalidCategory");
//                return;
//            }

//            SelectRandomWord(parameters.WordToGuess);
//            await hubContext.Clients.All.SendAsync("GameStarted", GetGuessedWord().Length);

//            GameStarted?.Invoke(this, new GameStartedEventArgs(parameters));
//        }

//        public async Task MakeMove(GameMove move)
//        {
//            char letter = char.ToUpper(move.GuessLetter);
//            bool isCorrect = GuessLetterLogic(letter);

//            await hubContext.Clients.All.SendAsync("GuessResult", letter, isCorrect);

//            bool isGameOver = IsGameOver(out bool isWin);
//            if (isGameOver)
//            {
//                if (isWin)
//                {
//                    await hubContext.Clients.All.SendAsync("GameOver", true, secretWord);
//                }
//                else
//                {
//                    await hubContext.Clients.All.SendAsync("GameOver", false, secretWord);
//                }

//                GameEnded?.Invoke(this, new GameEndedEventArgs(isWin ? GameResult.Win : GameResult.Loss));
//            }
//        }

//        private void SelectRandomWord(string category)
//        {
//            List<string> wordsInCategory = wordCategories[category];
//            secretWord = wordsInCategory[rand.Next(wordsInCategory.Count)].ToUpper();
//        }

//        private bool GuessLetterLogic(char letter)
//        {
//            if (!char.IsLetter(letter) || guessedLetters.Contains(letter))
//            {
//                return false;
//            }

//            guessedLetters.Add(letter);
//            return secretWord.Contains(letter);
//        }

//        private bool IsGameOver(out bool isWin)
//        {
//            isWin = !secretWord.Any(c => !guessedLetters.Contains(c));
//            return isWin || GetIncorrectGuessCount() >= 6;
//        }

//        private string GetGuessedWord()
//        {
//            string guessedWord = "";
//            foreach (char letter in secretWord)
//            {
//                if (guessedLetters.Contains(letter))
//                {
//                    guessedWord += letter;
//                }
//                else
//                {
//                    guessedWord += "_";
//                }
//            }
//            return guessedWord;
//        }

//        private int GetIncorrectGuessCount()
//        {
//            int count = 0;
//            foreach (char letter in guessedLetters)
//            {
//                if (!secretWord.Contains(letter))
//                {
//                    count++;
//                }
//            }
//            return count;
//        }
//    }
//}
