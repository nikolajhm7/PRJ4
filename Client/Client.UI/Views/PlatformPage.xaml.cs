using Client.Libary.Models;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;

namespace Client.UI.Views
{
    public partial class PlatformPage : ContentPage
    {
        public PlatformPage(PlatformViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}