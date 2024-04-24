using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Libary.Models
{
    public partial class Game : ObservableObject
    {
        [ObservableProperty] private int _gameId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private bool _playable = true;
        [ObservableProperty] private string _image;

        public Game()
        {
        }

        public void setImage()
        {
           Image = Name+".png";
        }
    }
}
