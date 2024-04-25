using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace Client.UI.Views
{
    public partial class ChatAppPage : ContentPage
    {
        public ChatAppPage(ChatAppViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }

}
