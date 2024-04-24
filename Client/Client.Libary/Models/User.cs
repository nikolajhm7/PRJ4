using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Libary.Models
{
    public partial class User:ObservableObject
    {
        private static User _instance = null;
        [ObservableProperty]
        string _username = Preferences.Get("username", defaultValue: string.Empty);

        private User()
        {
            Random random = new Random();
            avatar = random.Next(1,4);
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

        public int avatar { get; set; }
    }
}

