using System;
using Microsoft.Maui.Controls;
using Client.UI.ViewModels.Manager;
using System.Globalization;
using System.Diagnostics;

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
    public class BackgroundColorConverter : IMultiValueConverter
    {
        public BackgroundColorConverter()
        {

        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("Convert Initiated");
            if (values != null)
            {
                string currentPlayer = (string)values[0];
                string frontPlayer = (string)values[1];

                Debug.WriteLine($"Current Player: {currentPlayer}");
                Debug.WriteLine($"Front Player: {frontPlayer}");

                if (currentPlayer == frontPlayer)
                {
                    Debug.WriteLine("Setting background color to white.");
                    return Color.FromRgb(230, 230, 230); // White background color for the front player
                }
            }

            // Default background color for other players
            Debug.WriteLine("Setting default background color.");
            return Color.FromRgb(25, 25, 25);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("ConvertBack called");
            throw new NotImplementedException();
        }
    }

}

