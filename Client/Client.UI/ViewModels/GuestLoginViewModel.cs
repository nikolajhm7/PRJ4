using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Client.Library.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Client.Library.Services.Interfaces;
using Client.UI.Views;

namespace Client.UI.ViewModels
{
    public partial class GuestLoginViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly INavigationService _navigationService;
        private IJwtTokenService _jwtTokenService;

        public GuestLoginViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration,
            INavigationService navigationService, IJwtTokenService jwtTokenService)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
            _configuration = configuration;
            _navigationService = navigationService;
            _jwtTokenService = jwtTokenService;
        }

        [ObservableProperty] string _username = "Test123";

        [RelayCommand]
        public async Task GoBack()
        {
            await _navigationService.NavigateBack();
        }

        [RelayCommand]
        public async Task MakeNewUser()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                await Shell.Current.DisplayAlert("Fejl", "Brugernavn skal udfyldes", "OK");
                return;
            }
            string pattern = @"^[A-Za-z0-9]{2,20}$";

            if (!Regex.IsMatch(Username, pattern))
            {
                await Shell.Current.DisplayAlert("Fejl", "Brugernavnet skal være mellem 2 og 20 tegn og må kun indeholde bogstaver og tal", "OK");
                return;
            }

            var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/login-as-guest",
                new {GuestName = Username });
            
            if (_jwtTokenService.SetTokensFromResponse(response))
            {
                await Shell.Current.DisplayAlert("Succes", "Du er nu logget ind som gæst", "OK");
                await _navigationService.NavigateToPage(nameof(JoinPage));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                await Shell.Current.DisplayAlert("Fejl", $"Der opstod en fejl: {errorResponse}", "OK");
            }
        }
    }
}