using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Client.UI.ViewModels;
using Client.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui;
using Client.UI.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<PlatformViewModel>();
            builder.Services.AddSingleton<PlatformPage>();

            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginPage>();
            // Singletons er når der kun er 1 indstans som vi navigere tilbage til og som består

            builder.Services.AddTransient<TestViewModel>();
            builder.Services.AddTransient<TestPage>();

            builder.Services.AddTransient<LobbyPage>();
            builder.Services.AddTransient<LobbyViewModel>();

            // Når det er Transient betyder det at den laver nye kopier hver gang man navigere til siden
            // All sider som er midlertidige skal være Transient

            builder.Services.AddSingleton<ConnectionService>();
            builder.Services.AddSingleton<LobbyService>();
            builder.Services.AddSingleton<FriendsService>();

            builder.Services.AddSingleton<ConnectionService>();




            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("ApiHttpClient")
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();

#if DEBUG
            builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
    
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
