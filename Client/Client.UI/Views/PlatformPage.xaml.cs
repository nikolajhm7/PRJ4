using Client.Library.Models;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels;
using Client.Library.DTO;


namespace Client.UI.Views
{
    public partial class PlatformPage : ContentPage
    {
        public PlatformPage(PlatformViewModel vm)
        {
            InitializeComponent();
            this.Appearing += OnPageAppearing;
            BindingContext = vm;
    }

            
   
        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Call the base implementation first
            if (BindingContext is PlatformViewModel viewModel)
            {
                viewModel.OnPageAppearing();
            }

            // Perform actions when the page appears
        }
    }
}