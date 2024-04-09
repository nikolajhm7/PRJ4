using Client.UI.Views;

namespace Client.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(PlatformPage), typeof(PlatformPage));
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
            Routing.RegisterRoute(nameof(LobbyPage), typeof(LobbyPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(JoinPage), typeof(JoinPage));
            //Med "nameof" laver den selv sidens navn til en string og den er dermed mere dynamisk
        }
    }
}
