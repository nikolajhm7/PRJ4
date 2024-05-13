using System;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels.Manager;

namespace Client.UI.Games
{
    public partial class HangmanPage : ContentPage
    {
        HangmanViewModel _vm;
        public HangmanPage(ViewModelFactory viewModelFactory)
        {
            _vm = viewModelFactory.GetHangmanViewModel();
            this.BindingContext = _vm;
            this.Appearing += OnPageAppearing;
            InitializeComponent();
        }

        
        private async void OnPageAppearing(object sender, EventArgs e)
        {
            // Call the base implementation first
            if (BindingContext is HangmanViewModel viewModel)
            {
                await viewModel.OnPageAppearing();
            }
        }
    }

}

