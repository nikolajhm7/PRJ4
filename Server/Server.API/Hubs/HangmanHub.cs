using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Server.API.GameLogic;

namespace Server.API.Hubs
{
    public class HangmanHub : Hub
    {
        private readonly HangmanGame hangmanGame;

        // Constructor with HangmanGame parameter
        public HangmanHub(HangmanGame hangmanGame)
        {
            this.hangmanGame = hangmanGame;
        }

        // Method to start the game
        public async Task StartGame(string category)
        {
            // Start the game and send the result to the client
            await hangmanGame.StartGame(category, Context.ConnectionId);
        }

        // Method to handle letter guesses
        public async Task GuessLetter(char letter)
        {
            // Process the letter guess
            await hangmanGame.GuessLetter(letter);
        }
    }
}