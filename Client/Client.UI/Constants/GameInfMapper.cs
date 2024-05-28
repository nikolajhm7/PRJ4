using Client.Library.Models;
using Client.UI.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Constants
{
    public static class GameInfMapper
    {
        public static Dictionary<int, GameInfo> GameInfoDictionary = new Dictionary<int, GameInfo>();

        static GameInfMapper()
        {
            GameInfoDictionary.Add(1, new GameInfo { ImagePath = "hangman.png", Name = "Hangman", Route = nameof(HangmanPage)});
            GameInfoDictionary.Add(2, new GameInfo { ImagePath = "krydsogbolle.png", Name = "Krydsogbolle" });
            GameInfoDictionary.Add(3, new GameInfo { ImagePath = "stensakspapir.png", Name = "Stensakspapir" });
        }
    }
}
