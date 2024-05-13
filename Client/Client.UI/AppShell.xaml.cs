using Client.UI.Views;
using Client.UI.Games;

namespace Client.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(NewUserPage), typeof(NewUserPage));
            Routing.RegisterRoute(nameof(PlatformPage), typeof(PlatformPage));
            Routing.RegisterRoute(nameof(LobbyPage), typeof(LobbyPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(JoinPage), typeof(JoinPage));
            Routing.RegisterRoute(nameof(GuestLoginPage), typeof(GuestLoginPage));
            //Games:
            Routing.RegisterRoute(nameof(HangmanPage), typeof(HangmanPage));
        }
    }
}
