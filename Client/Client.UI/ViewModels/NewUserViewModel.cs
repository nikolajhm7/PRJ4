using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Client.Library.Services;
using Client.Library.Services.Interfaces;
using Client.UI.Views;

namespace Client.UI.ViewModels
{
    public partial class NewUserViewModel: ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly INavigationService _navigationService;
        public NewUserViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, INavigationService navigationService)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
            _configuration = configuration;
            _navigationService = navigationService;
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
                    Console.WriteLine($"Oprettelse fejlede med status kode {response.StatusCode}: {errorResponse}");
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

        [ObservableProperty]
        string _username = "";
        [ObservableProperty]
        string _password = "";
        [ObservableProperty]
        string _email = "";

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

            if (string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Fejl", "Adgangskode skal udfyldes", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                await Shell.Current.DisplayAlert("Fejl", "Email skal udfyldes", "OK");
                return;
            }
            else if (await Check(_username, _password, _email))
            {
                await Shell.Current.DisplayAlert("Succses", $"{Username} was created","OK");
                await _navigationService.NavigateBack();
                return;
            }
        }
    }
}
