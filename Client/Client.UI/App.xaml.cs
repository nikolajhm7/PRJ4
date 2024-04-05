using System;
using Microsoft.Maui.Controls;

namespace Client.UI
{
    public partial class App : Application
    {
        
        public static string ApiUrl = "http://localhost:5008";
        public IServiceProvider ServiceProvider { get; }
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            ServiceProvider = serviceProvider;

            MainPage = new AppShell();


        }
    }
}
