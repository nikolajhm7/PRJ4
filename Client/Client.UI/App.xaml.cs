using System;
using Client.UI.Views;
using Microsoft.Maui.Controls;

namespace Client.UI
{
    public partial class App : Application
    {
        
        public IServiceProvider ServiceProvider { get; }
        private static LoginPage _loginPage;
        public App(IServiceProvider serviceProvider, LoginPage loginPage)
        {
            InitializeComponent();
            ServiceProvider = serviceProvider;
            _loginPage = loginPage;
            
            MainPage = new AppShell();

            
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await MainPage.Navigation.PushModalAsync(_loginPage);
        }

        public static async Task ShowLoginPage()
        {
            // Metode til at vise LoginPage fra hvor som helst i appen
            await Current.MainPage.Navigation.PushModalAsync(_loginPage);
        }
    }
}
