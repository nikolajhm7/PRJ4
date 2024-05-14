using Client.Library.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Library.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Client.Library.Models;
using System.Collections.ObjectModel;
using Client.Library.Services;
using Client.Library;
using Newtonsoft.Json;


namespace Client.UI.ViewModels
{
    public partial class ShopViewModel : ObservableObject
    {
        #region Propeties
        private ObservableCollection<Game> games;

        public ObservableCollection<Game> Games
        {
            get { return games; }
            set { SetProperty(ref games, value); }
        }

        [ObservableProperty] private string? _username;

        #endregion

        #region Setup
        private IJwtTokenService _jwtTokenService;
        private readonly IApiService _apiService;


        private readonly INavigationService _navigationService;
        public ShopViewModel(IApiService apiService, INavigationService navigationService, IJwtTokenService jwtTokenService)
        {
            _navigationService = navigationService;
            _jwtTokenService = jwtTokenService;
            _apiService = apiService;

        }


        #endregion

        public async void OnPageAppearing()
        {
            Username = _jwtTokenService.GetUsernameFromToken();
            await pullGames();
        }

        #region Commands
        [RelayCommand]
        public async Task GoBack()
        {
            await _navigationService.NavigateBack();
        }
        public void SetImagesForGames()
        {
            foreach (var game in Games)
            {
                game.setImage();
            }
        }
        [RelayCommand]
        public async Task pullGames()
        {
            string endpoint = $"/Game/getAllGames";
            var response = await _apiService.MakeApiCall(endpoint, HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(jsonResponse);
                SetImagesForGames();
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Kunne ikke hente spil", "OK");
            }

        }


        #endregion


    }
}