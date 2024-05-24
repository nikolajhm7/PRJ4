using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Library.DTO
{
    public class GameUserDTO : ObservableObject
    {
        public string UserName { get; set; }
        public int GameId { get; set; }
    }
}
