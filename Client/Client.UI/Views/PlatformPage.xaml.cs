using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

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