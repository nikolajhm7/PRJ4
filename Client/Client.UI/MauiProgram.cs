using System.Diagnostics;
using System.Text;
using Client.UI.DTO;
using Client.UI.ViewModels;
using Client.UI.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Client.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

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
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "Logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();
            
            builder.Logging.AddSerilog();
            
            var logFilePath = Path.Combine(FileSystem.AppDataDirectory, "Logs/log-.txt");
            Console.WriteLine($"Logfil sti: {logFilePath}");
            
            Log.Information("App started");
            
         
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            builder.Configuration.AddConfiguration(configuration);

            builder.Services.AddSingleton<PlatformViewModel>();
            builder.Services.AddSingleton<PlatformPage>();

            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginPage>();
            // Singletons er når der kun er 1 indstans som vi navigere tilbage til og som består

            builder.Services.AddTransient<TestViewModel>();
            builder.Services.AddTransient<TestPage>();

            builder.Services.AddTransient<LobbyPage>();
            builder.Services.AddTransient<LobbyViewModel>();

            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<JoinPage>();
            builder.Services.AddTransient<JoinViewModel>();

            // Når det er Transient betyder det at den laver nye kopier hver gang man navigere til siden
            // All sider som er midlertidige skal være Transient

            builder.Services.AddSingleton<LobbyService>();
            builder.Services.AddSingleton<FriendsService>();


            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("ApiHttpClient")
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            var app = builder.Build();
            
            Task.Run(() => UploadLogsAsync());

            return app;
        }

        public static async Task UploadLogsAsync()
        {
            // Tjek om enheden har internetadgang og er på Wi-Fi
            if (!IsNetworkAvailable())
            {
                return;
            }

            var logsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Logs");
            var logFiles = Directory.GetFiles(logsDirectory, "*.txt"); // Antager at logs er i .txt filer
            Console.WriteLine("Log files:");
            foreach (var logFile in logFiles)
            {
                
                try
                {
                    var logLines = await File.ReadAllLinesAsync(logFile);
                    var jsonLogContent = ConvertLogLinesToJson(logLines); // Konverter til JSON
                    if (await SendLogsToServer(jsonLogContent)) // Send som JSON
                    {
                        
                        //File.Delete(logFile); // Slet filen, hvis uploadet lykkes
                    }

                }
                catch (Exception ex)
                {
                    // Log fejlen
                    Log.Error(ex, "Fejl under upload af logfil. Fil: {logFile}", logFile);
                }
            }
        }

        private static bool IsNetworkAvailable()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet &&
                   Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi);
        }

        private static async Task<bool> SendLogsToServer(string jsonContentString)
        {
            try
            {
                var jsonContent = new StringContent(jsonContentString, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(App.ApiUrl + "/logs", jsonContent);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fejl under upload af logfil");
                return false;
            }
        }
        
        private static string ConvertLogLinesToJson(IEnumerable<string> logLines)
        {
            var logEntries = logLines.Select(logLine =>
            {
                var parts = logLine.Split(' ');
                var timestamp = parts[0] + " " + parts[1];
                var level = parts[2].Trim('[', ']');
                var message = string.Join(" ", parts.Skip(4));

                return new LogEntryDTO
                {
                    Timestamp = DateTime.Parse(timestamp),
                    Level = level,
                    Message = message
                };
            }).ToList();

            return JsonConvert.SerializeObject(logEntries); // Konverterer hele listen til JSON
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
