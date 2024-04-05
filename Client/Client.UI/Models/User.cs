using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Models
{
    public class User
    {
        private static User _instance = null;
        private string _username = "test";

        private User()
        {
            games.Add("hangman.png");
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
        public double coins { get; set; }
    }
}

