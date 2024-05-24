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

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            // Check if the DataContext is your ViewModel
            if (BindingContext is HangmanViewModel viewModel)
            {
                // Ensure the command can be executed
                if (viewModel.GuessLetterCommand.CanExecute(viewModel.Letter) && viewModel.Letter != null)
                {
                    viewModel.GuessLetterCommand.Execute(viewModel.Letter);
                }
            }
        }
    }

}

