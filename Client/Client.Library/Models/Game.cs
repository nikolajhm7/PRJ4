using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Library.Models
{
    public partial class Game : ObservableObject
    {
        [ObservableProperty] private int _gameId;
        [ObservableProperty] private string _name=null;
        [ObservableProperty] private string _image;
        [ObservableProperty] private int maxPlayers;

        public Game()
        {
        }

        public void setImage()
        {
            if (Name != null)
            {
                Image = Name + ".png";
            }
            else
            {
                Image = "locked.png";
            }
            
        }
    }
}