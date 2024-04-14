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

namespace Client.UI.ViewModels
{
    public partial class NewUserViewModel: ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public NewUserViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
            _configuration = configuration;
        }

        public async Task<bool> Check(string username, string password, string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/makeNewUser", new { UserName = username, Email = email , Password = password});
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
        string _username = "test123";
        [ObservableProperty]
        string _password = "test123test123";
        [ObservableProperty]
        string _email = "test@123.com";

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
            if (await Check(Username, Password, Email))
            {
                await Shell.Current.DisplayAlert("Succses", $"{Username} was created", "OK");
                //await Shell.Current.GoToAsync("LoginPage");
                return;
            }
            else
            { 
                await Shell.Current.DisplayAlert("Fejl", "Oprettelse fejlede", "OK");
            }
        }
    }
}
