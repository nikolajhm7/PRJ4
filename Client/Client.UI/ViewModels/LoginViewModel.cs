using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.UI.Views;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using Client.UI.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Client.UI.Services;
using Client.UI.Views;

namespace Client.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;
    
    private readonly ILogger<LoginViewModel> _logger;

    private readonly NavigationService _navigationService;
    public LoginViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoginViewModel> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
        
        _configuration = configuration;

        _navigationService = new NavigationService();
        
        _logger = logger;

        IsAlreadyAuthenticated();
    }

    private async void IsAlreadyAuthenticated()
    {
        if (await IsUserAuthenticated())
        {
            await _navigationService.NavigateToPage(nameof(PlatformPage));
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
            User.Instance.Username = LoginUsername;
            await _navigationService.NavigateToPage(nameof(PlatformPage));
        }
        else
        {
            await Shell.Current.DisplayAlert("Fejl", "Login fejlede", "OK");
        }
    }

    [RelayCommand]
    public async Task JoinAsGuest()
    {
        await _navigationService.NavigateToPage(nameof(JoinAsGuest));
    }

    private async Task<bool> IsUserAuthenticated()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration["ConnectionSettings:ApiUrl"] + "/checkLoginToken");

            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }

        return false;
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
                    Preferences.Set("username", username);
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