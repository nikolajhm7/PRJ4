namespace Server.API.GameLogic
{
    /*
    // Define the basic operations and events common to all games
    public interface IGame
    {
        // Event raised when the game starts
        event EventHandler<GameStartedEventArgs> GameStarted;

        // Event raised when the game ends
        event EventHandler<GameEndedEventArgs> GameEnded;

        // Method to start a new game with the specified parameters
        Task StartGame(GameParameters parameters);

        // Method to make a move or perform an action in the game
        Task MakeMove(GameMove move);
    }

    // Custom event arguments for the GameStarted event
    public class GameStartedEventArgs : EventArgs
    {
        public GameParameters Parameters { get; }

        public GameStartedEventArgs(GameParameters parameters)
        {
            Parameters = parameters;
        }
    }

    // Custom event arguments for the GameEnded event
    public class GameEndedEventArgs : EventArgs
    {
        public GameResult Result { get; }

        public GameEndedEventArgs(GameResult result)
        {
            Result = result;
        }
    }

    // Class representing the parameters used to start a game
    public class GameParameters
    {
        // Define properties for game parameters relevant to Hangman
        public string WordToGuess { get; set; }
        public int MaxIncorrectGuesses { get; set; }
        public List<string> Players { get; set; }

        // Add any additional properties as needed for other games

    }

    // Class representing a move made by a player in the game
    public class GameMove
    {
        // Defining properties for game moves 
        public string PlayerId { get; set; } // For multiplayer games
        public char GuessLetter { get; set; } // For Hangman game
    }

    // Enum representing the result of a game (e.g., win, loss, draw, etc.)
    public enum GameResult
    {
        Win,
        Loss,
        Draw
    }*/
}