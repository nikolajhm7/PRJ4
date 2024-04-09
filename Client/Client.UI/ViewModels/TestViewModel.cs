using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.UI.Models;
using Client.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Serilog;

namespace Client.UI.ViewModels
{
    public partial class TestViewModel :ObservableObject
    {
        public TestViewModel()
        {
            username = "";
        }

        [ObservableProperty]
        string username;
        //Vi bruger mvvm toolkit til at holde styr på at objektet ændret sig

        [RelayCommand]
        private async Task NavUsername(string s)
        {
            User.Instance.Username = s;

            await Shell.Current.GoToAsync($"PlatformPage"); 
        }
    }
}