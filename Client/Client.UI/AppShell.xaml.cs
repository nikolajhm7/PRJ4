using Client.UI.Views;

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
            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            //Med "nameof" laver den selv sidens navn til en string og den er dermed mere dynamisk
        }
    }
}
