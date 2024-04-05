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

namespace Client.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    public ICommand LoginOnPlatformCommand { get; }
    public LoginViewModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
        
        LoginOnPlatformCommand = new Command(async () => await LoginOnPlatform());
        
        IsAlreadyAuthenticated();
    }
    
    private async void IsAlreadyAuthenticated()
    {
        if (await IsUserAuthenticated())
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
            await Shell.Current.GoToAsync("PlatformPage");
            
        }
        else
        {
            await Shell.Current.DisplayAlert("Fejl", "Login fejlede", "OK");
        }
    }
    
    private async Task<bool> IsUserAuthenticated()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, App.ApiUrl + "/checkLoginToken");
            
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
        
        return true;
        try
        {
            var response = await _httpClient.PostAsJsonAsync(App.ApiUrl + "/login", new { Username = username, Password = password });

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
                Console.WriteLine($"Login fejlede med status kode {response.StatusCode}: {errorResponse}");
            }
        }
        catch (HttpRequestException e)
        {
            // Specifik handling for HTTP-relaterede fejl
            Console.WriteLine($"En HTTP fejl opstod: {e.Message}");
            // Overvej at vise en brugervenlig fejlmeddelelse
        }
        catch (Exception e)
        {
            // Generel exception handling
            Console.WriteLine($"En uventet fejl opstod: {e.Message}");
            // Overvej at vise en brugervenlig fejlmeddelelse
        }
        return false;
    }


}