using System.Text;
using System.Text.RegularExpressions;
using Client.Library.Interfaces;
using Client.Library.DTO;
using Client.UI.Managers;
using Client.UI.ViewModels;
using Client.UI.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Client.Library.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Client.Library.Games;
using Client.Library;
using Client.Library.Services.Interfaces;

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

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            #region Services

            #region Pages

            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<LoadingViewModel>();

            builder.Services.AddTransient<PlatformViewModel>();
            builder.Services.AddTransient<PlatformPage>();

            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginPage>();

            builder.Services.AddTransient<GuestLoginViewModel>();
            builder.Services.AddTransient<GuestLoginPage>();

            builder.Services.AddTransient<LobbyPage>();
            builder.Services.AddTransient<LobbyViewModel>();

            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddSingleton<JoinPage>();
            builder.Services.AddSingleton<JoinViewModel>();

            builder.Services.AddTransient<NewUserPage>();
            builder.Services.AddTransient<NewUserViewModel>();

            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<GameViewModel>();

            builder.Services.AddTransient<FriendsViewModel>();

            #endregion

            builder.Configuration.AddConfiguration(configuration);
            builder.Services.AddSingleton<ILobbyService, LobbyService>();
            builder.Services.AddSingleton<IFriendsService, FriendsService>();
            builder.Services.AddSingleton<IHangmanService, HangmanService>();
            builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
            builder.Services.AddSingleton<IPreferenceManager, PreferenceManager>();
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddHttpClient("ApiHttpClient");

            #endregion


#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            var jwtTokenService = app.Services.GetRequiredService<IJwtTokenService>();


            Task.Run(() => UploadLogsAsync(configuration, jwtTokenService));

            return app;
        }

        private static bool IsNetworkAvailable()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet &&
                   Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi);
        }

        #region Logging

        public static async Task UploadLogsAsync(IConfiguration configuration, IJwtTokenService jwtTokenService)
        {
            // Tjek om enheden har internetadgang og er på Wi-Fi
            if (!IsNetworkAvailable() && !await jwtTokenService.IsAuthenticated())
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

        private static async Task<bool> SendLogsToServer(string jsonContentString, IConfiguration _configuration)
        {
            try
            {
                var jsonContent = new StringContent(jsonContentString, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                var response =
                    await httpClient.PostAsync(_configuration["ConnectionSettings:ApiUrl"] + "/logs", jsonContent);
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
}

#endregion

