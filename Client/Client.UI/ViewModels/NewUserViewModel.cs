using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Client.Library.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Client.UI.ViewModels
{
    public partial class NewUserViewModel: ObservableObject
    {
        #region Properties

        [ObservableProperty]
        string _username = "";
        [ObservableProperty]
        string _password = "";
        [ObservableProperty]
        string _email = "";

        #endregion

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly INavigationService _navigationService;
        private ILogger<NewUserViewModel> _logger;
        public NewUserViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService, ILogger<NewUserViewModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
            _configuration = configuration;
            _navigationService = navigationService;
            _logger = logger;
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await _navigationService.NavigateBack();
        }

        #region Checking and making user

        [RelayCommand]
        public async Task MakeNewUser()
        {
            _logger.LogInformation("Make new user clicked");
            if (string.IsNullOrWhiteSpace(Username))
            {
                _logger.LogWarning("Username is empty");
                await Shell.Current.DisplayAlert("Fejl", "Brugernavn skal udfyldes", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _logger.LogWarning("Password is empty");
                await Shell.Current.DisplayAlert("Fejl", "Adgangskode skal udfyldes", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                _logger.LogWarning("Email is empty");
                await Shell.Current.DisplayAlert("Fejl", "Email skal udfyldes", "OK");
                return;
            }
            else if (await Check(_username, _password, _email))
            {
                _logger.LogInformation("User created");
                await Shell.Current.DisplayAlert("Succses", $"{Username} was created", "OK");
                await _navigationService.NavigateBack();
                return;
            }
        }
        public async Task<bool> Check(string username, string password, string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/makeNewUser", new { UserName = username, Password = password, Email = email });
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync(); 
                    _logger.LogWarning($"Failed to create user: {errorResponse}");
                    
                }

            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Oprettelse fejlede: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError($"En fejl opstod: {e.Message}");
            }
            return false;
        }
    }
    #endregion

}
