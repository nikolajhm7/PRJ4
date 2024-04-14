using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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
            
            builder.Services.AddTransient<TestViewModel>();
            builder.Services.AddTransient<TestPage>();

            builder.Services.AddTransient<LobbyPage>();
            builder.Services.AddTransient<LobbyViewModel>();

            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddSingleton<JoinPage>();
            builder.Services.AddSingleton<JoinViewModel>();

            builder.Services.AddTransient<NewUserPage>();
            builder.Services.AddTransient<NewUserViewModel>();

            builder.Services.AddSingleton<LobbyService>();
            builder.Services.AddSingleton<FriendsService>();

            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("ApiHttpClient")
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            var app = builder.Build();
            
            Task.Run(() => UploadLogsAsync(configuration));

            return app;
        }

        public static async Task UploadLogsAsync(IConfiguration configuration)
        {
            // Tjek om enheden har internetadgang og er på Wi-Fi
            if (!IsNetworkAvailable())
            {
                return;
            }

            var logsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Logs");
            var logFiles = Directory.GetFiles(logsDirectory, "*.txt"); // Antager at logs er i .txt filer
            foreach (var logFile in logFiles)
            {
                
                try
                {
                    var logLines = await File.ReadAllLinesAsync(logFile);
                    var jsonLogContent = ConvertLogLinesToJson(logLines); // Konverter til JSON
                    if (await SendLogsToServer(jsonLogContent, configuration)) // Send som JSON
                    {
                        
                        File.Delete(logFile); // Slet filen, hvis uploadet lykkes
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

        private static async Task<bool> SendLogsToServer(string jsonContentString, IConfiguration _configuration)
        {
            try
            {
                var jsonContent = new StringContent(jsonContentString, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(_configuration["ConnectionSettings:ApiUrl"] + "/logs", jsonContent);
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
            var logEntries = new List<LogEntryDTO>();
            LogEntryDTO currentEntry = null;
            var logStartPattern = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \+\d{2}:\d{2} \[(\w+)\]");

            foreach (var line in logLines)
            {
                var match = logStartPattern.Match(line);
                if (match.Success)
                {
                    if (currentEntry != null)
                    {
                        logEntries.Add(currentEntry);
                    }

                    var timestamp = DateTime.Parse(match.Value.Substring(0, 23));
                    var level = match.Groups[1].Value;
                    var message = line.Substring(match.Value.Length).TrimStart();

                    currentEntry = new LogEntryDTO
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Message = message
                    };
                }
                else if (currentEntry != null)
                {
                    // Dette er en fortsættelse af den nuværende besked over flere linjer
                    currentEntry.Message += "\n" + line;
                }
            }

            if (currentEntry != null)
            {
                logEntries.Add(currentEntry);
            }

            return JsonConvert.SerializeObject(logEntries);
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
