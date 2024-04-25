using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Library.Interfaces;
using Client.Library.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Library.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace Client.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {

        private readonly IApiService _apiService;
        public SettingsViewModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task Addgame(Game g)
        {
            //string endpoint = "/Game/addGameForUser";
            //new httpContent()
            //var response = await _apiService.MakeApiCall(endpoint, HttpMethod.Post,new httpContent{ Username = User.Instance.Username, g.GameId});

            //if (response.IsSuccessStatusCode)
            //{
            //    string jsonResponse = await response.Content.ReadAsStringAsync();
            //    Games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(jsonResponse);
            //}
            //else
            //{
            //    await Shell.Current.DisplayAlert("Fejl", "Kunne ikke hente spil", "OK");
            //}
        }

    }
}
