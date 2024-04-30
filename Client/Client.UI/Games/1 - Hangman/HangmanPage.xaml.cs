using System;
using Microsoft.Maui.Controls;

namespace Client.UI.Games
{
    public partial class HangmanPage : ContentPage
    {
        public HangmanPage(HangmanViewModel vm)
        {
            this.BindingContext = vm;
            this.Appearing += OnPageAppearing;
            InitializeComponent();
        }

        
        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Call the base implementation first
            if (BindingContext is HangmanViewModel viewModel)
            {
                viewModel.OnPageAppearing();
            }

        }

    }

}

