using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views
{
    public partial class GamePage : ContentPage
    {
        public GamePage(GameViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}