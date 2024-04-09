using Server.API.GameLogic;

namespace Server.API.Hubs
{
    // Generic interface for SignalR integration with games
    public interface IGameHub<TGame, TMove>
        where TGame : IGame
        //where TMove : IGameMove
    {
        // Method to start a new game
        Task StartGame(string gameId, GameParameters parameters);

        // Method to make a move in the game
        Task MakeMove(string gameId, TMove move);
    }
}
