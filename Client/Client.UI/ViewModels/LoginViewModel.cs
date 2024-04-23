using System.Net.Http.Json;
using Client.Library.Interfaces;
using Client.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Client.Library.Services;
using Client.Library.Services.Interfaces;

namespace Client.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;
    
    private readonly ILogger<LoginViewModel> _logger;

    private readonly INavigationService _navigationService;
    
    private IJwtTokenService _jwtTokenService;
    
    private IPreferenceManager _preferenceManager;
    
    private IApiService _apiService;
    public LoginViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoginViewModel> logger, IJwtTokenService jwtTokenService, IPreferenceManager preferenceManager, INavigationService navigationService, IApiService apiService)
    {
        _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
        
        _configuration = configuration;

        _navigationService = navigationService;
        
        _logger = logger;
        
        _jwtTokenService = jwtTokenService;
        
        _preferenceManager = preferenceManager;
        
        _apiService = apiService;

        IsAlreadyAuthenticated();
    }

    private async void IsAlreadyAuthenticated()
    {
        if (await _jwtTokenService.IsAuthenticated())
        {
            await _navigationService.NavigateToPage($"//{nameof(PlatformPage)}");
        }
    }
    [ObservableProperty]
    private string _loginUsername = string.Empty;
   
    [ObservableProperty]
    private string _loginPassword = string.Empty;

    [RelayCommand]
    async Task GoToNewUser()
    {
        await _navigationService.NavigateToPage(nameof(NewUserPage));
    }

    [RelayCommand]
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
            await _navigationService.NavigateToPage($"//{nameof(PlatformPage)}");
        }
        else
        {
            await Shell.Current.DisplayAlert("Fejl", "Login fejlede", "OK");
        }
    }

    [RelayCommand]
    public async Task JoinAsGuest()
    {
        await _navigationService.NavigateToPage(nameof(GuestLoginPage));
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/login", new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                if (_jwtTokenService.SetTokensFromResponse(response))
                {
                    _preferenceManager.Set("username", username);
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