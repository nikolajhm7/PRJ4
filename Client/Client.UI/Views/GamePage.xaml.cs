using System;
using System.Globalization;
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
            if (BindingContext is GameViewModel viewModel)
            {
                viewModel.OnPageAppearing();
            }

        }
        public class BackgroundColorConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                // Check if values are provided and are of correct types
                if (values != null && values.Length == 2 && values[0] != null && values[1] != null && values[1] is string)
                {
                    string currentPlayer = (string)values[0];
                    string frontPlayer = (string)values[1];

                    // Check if the current player is the front of the queue
                    if (currentPlayer == frontPlayer)
                    {
                        return Color.FromRgb(255, 255, 255); // White background color for the front player
                    }
                }

                // Default background color for other players
                return Color.FromRgb(25, 25, 25);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }
}