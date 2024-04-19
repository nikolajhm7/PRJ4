using System;
using Client.UI.Views;
using Microsoft.Maui.Controls;

namespace Client.UI
{
    public partial class App : Application
    {
        
        public IServiceProvider ServiceProvider { get; }
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            ServiceProvider = serviceProvider;
            
            MainPage = new AppShell();

            
        }
    }
}
