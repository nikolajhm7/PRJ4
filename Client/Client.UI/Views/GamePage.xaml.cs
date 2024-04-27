using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views
{
    public partial class GamePage : ContentPage
    {

        public GamePage(GameViewModel vm)
        {
            this.BindingContext = vm;
            this.Appearing += OnPageAppearing;
            InitializeComponent();
        }
        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Call the base implementation first
            if (BindingContext is PlatformViewModel viewModel)
            {
                viewModel.OnPageAppearing();
            }
        }

    }
}