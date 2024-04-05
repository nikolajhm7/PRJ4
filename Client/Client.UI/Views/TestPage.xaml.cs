
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;
namespace Client.UI.Views
{
    public partial class TestPage : ContentPage
    {
        public TestPage(TestViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
