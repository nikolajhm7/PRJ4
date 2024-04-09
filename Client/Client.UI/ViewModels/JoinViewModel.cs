using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Client.UI.Models;
using Client.UI.Services;
using System.Diagnostics;

namespace Client.UI.ViewModels
{
    public partial class JoinViewModel : ObservableObject
    {
        public JoinViewModel()
        { 
        }
        [RelayCommand]
       public async Task GoToLobby()
        {
            await Shell.Current.GoToAsync($"LobbyPage");
        }
    }
}