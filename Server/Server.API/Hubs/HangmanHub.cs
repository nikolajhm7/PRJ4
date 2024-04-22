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

        // Chat-function for test?
        public async Task SendMessage(string message)
        {
            Console.WriteLine(message);
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        // Method to start the game
        public async Task StartGame(GameParameters category)
        {
            // Start the game and send the result to the client
            await hangmanGame.StartGame(category);
        }

        // Method to handle letter guesses
        public async Task MakeMove(GameMove letter)
        {
            // Process the letter guess
            await hangmanGame.MakeMove(letter);
        }

         public override async Task OnConnectedAsync()
        {
            // Add custom logic for when a client connects to the hub
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Add custom logic for when a client disconnects from the hub
            await base.OnDisconnectedAsync(exception);
        }

        //public override async Task OnReconnectedAsync()
        //{
        //    // Add custom logic for when a client reconnects to the hub
        //    await base.OnReconnectedAsync();
        //}

        //public override async Task OnClientTimeout(string connectionId, TimeSpan timeout)
        //{
        //    // Add custom logic for when a client's connection times out
        //    await base.OnClientTimeout(connectionId, timeout);
        //}

        //protected override void OnDisconnected(Exception exception)
        //{
        //    // Add custom logic for when a client disconnects from the hub (synchronous version)
        //    base.OnDisconnected(exception);
        //}

    }
}