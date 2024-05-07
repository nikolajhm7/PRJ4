using Server.API.Services.Interfaces;

namespace Server.API.Games
{
    public interface IHangmanLogic
    {
        public string SecretWord { get; }
        int StartGame();
        bool GuessLetter(char letter, out List<int> positions);
        bool IsGameOver();
        bool DidUserWin();
        List<char> GetGuessedLetters();
        public void SetQueue(Queue<string> userQueue);
        public Queue<string> GetQueue();
    }
}