using System;
using Microsoft.Maui.Controls;

namespace Client.UI.Games
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
