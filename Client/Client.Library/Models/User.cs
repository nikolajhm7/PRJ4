using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Library.Models
{
    public class User
    {
        private static User _instance = null;
        private string _username = Preferences.Get("username", defaultValue: string.Empty);

        private User()
        {
            games.Add(Path.GetFileNameWithoutExtension("/Resources/Images/hangman.png"+".png"));
            avatar = "charizard.png";
        }

        public static User Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new User();
                }
                return _instance;
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                }
            }
        }

        public ObservableCollection<string> games { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> friends { get; set; } = new ObservableCollection<string>();
        public string avatar { get; set; }
        public double coins { get; set; }
    }
}

