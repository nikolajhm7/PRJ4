using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
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
        }


        public async Task<bool> Check(string username, string password, string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_configuration["ConnectionSettings:ApiUrl"] + "/makeNewUser", new { Username = username, Password = password, Email = email });
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
        string _username;
        [ObservableProperty]
        string _password;
        [ObservableProperty]
        string _email;

        [RelayCommand]
        public async Task MakeNewUser()
        {
            if (string.IsNullOrWhiteSpace(_username))
            {
                await Shell.Current.DisplayAlert("Fejl", "Brugernavn skal udfyldes", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_password))
            {
                await Shell.Current.DisplayAlert("Fejl", "Adgangskode skal udfyldes", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_email))
            {
                await Shell.Current.DisplayAlert("Fejl", "Email skal udfyldes", "OK");
                return;
            }
            else (await Check(_username, _password,_email));
            {

            }
        }
    }
}
