using System.Net.Http.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.UI.Models;
using Client.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Client.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    public ICommand LoginOnPlatformCommand { get; }

    private readonly IConfiguration _configuration;
    
    private readonly ILogger<LoginViewModel> _logger;
    
    private readonly AuthenticationService _authenticationService;
    public LoginViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoginViewModel> logger, AuthenticationService authenticationService)
    {
        _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
        
        _configuration = configuration;
        
        _logger = logger;

        LoginOnPlatformCommand = new Command(async () => await LoginOnPlatform());
        
        _authenticationService = authenticationService;

        IsAlreadyAuthenticated();
    }

    private async void IsAlreadyAuthenticated()
    {
        if (await _authenticationService.IsUserAuthenticated())
        {
            await Shell.Current.GoToAsync("PlatformPage");
        }
    }

    private string _loginUsername = string.Empty;
    public string LoginUsername
    {
        get => _loginUsername;
        set => SetProperty(ref _loginUsername, value);
    }

    private string _loginPassword = string.Empty;
    public string LoginPassword
    {
        get => _loginPassword;
        set => SetProperty(ref _loginPassword, value);
    }

    public async Task LoginOnPlatform()
    {

        if (string.IsNullOrWhiteSpace(LoginUsername))
        {
            await Shell.Current.DisplayAlert("Fejl", "Brugernavn skal udfyldes", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(LoginPassword))
        {
            await Shell.Current.DisplayAlert("Fejl", "Adgangskode skal udfyldes", "OK");
            return;
        }

        if (await LoginAsync(LoginUsername, LoginPassword))
        {
            User.Instance.Username = LoginUsername;
            await Shell.Current.GoToAsync("PlatformPage");
        }
        else
        {
            await Shell.Current.DisplayAlert("Fejl", "Login fejlede", "OK");
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/login", new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var jsonResponseToken = (await response.Content.ReadAsStringAsync()).Trim();

                if (!string.IsNullOrWhiteSpace(jsonResponseToken))
                {
                    Preferences.Set("auth_token", jsonResponseToken);
                    return true;
                }

            }
            else
            {
                // Log fejlresponsen for diagnosticeringsformål
                var errorResponse = await response.Content.ReadAsStringAsync();
                    
                _logger.LogError($"Login fejlede med status kode {response.StatusCode}: {errorResponse}");
            }
        }
        catch (HttpRequestException e)
        {
            // Specifik handling for HTTP-relaterede fejl
            
            _logger.LogError($"En HTTP fejl opstod: {e.Message}");
            
        }
        catch (Exception e)
        {
            // Generel exception handling
            
            _logger.LogError($"En uventet fejl opstod: {e.Message}");
        }
        return false;
    }


}