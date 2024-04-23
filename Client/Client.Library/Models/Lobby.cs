using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Library.Models
{
    public class Lobby
    {
        public ObservableCollection<string> PlayerNames { get; set; } = new ObservableCollection<string>();
        public string LobbyId { get; set; }
    }
}
