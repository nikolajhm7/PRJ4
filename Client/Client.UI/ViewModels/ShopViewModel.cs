using Client.Library.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Client.Library.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Client.Library.Models;
using System.Collections.ObjectModel;
using Client.Library.Services;
using Client.Library;
using Client.Library.DTO;
using Newtonsoft.Json;
using System.Net.Http.Headers;


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
        private Game _selectedGame;
        [ObservableProperty] private bool _IsAddButtonVisible = true;




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
        [RelayCommand]
        public async void AddGame()
        {
            var username = _jwtTokenService.GetUsernameFromToken();

            string getEndpoint = $"/Game/getGamesForUser/{_jwtTokenService.GetUsernameFromToken()}";
            var getresponse = await _apiService.MakeApiCall(getEndpoint, HttpMethod.Get);


            if (!getresponse.IsSuccessStatusCode)
            {
                // Handle the case where the API call was not successful
                await Shell.Current.DisplayAlert($"Fejl", $"Kunne ikke tilføje spil", "OK");
                return;
            }

            // Read the response content as a string
            string responseContent = await getresponse.Content.ReadAsStringAsync();

            // Deserialize the response content into a list of games
            var games = JsonConvert.DeserializeObject<List<Game>>(responseContent);

            // Check if the game g exists in the list of games
            bool gameExists = games.Any(game => game.GameId == _selectedGame.GameId); // Assuming game ID is used for comparison

            if (!gameExists)
            {
                var gameUserDto = new GameUserDTO
                {
                    UserName = username,
                    GameId = _selectedGame.GameId // Assuming 'Game' object has an 'Id' property
                };
                // Serialize GameUserDTO object to JSON
                string jsonContent = JsonConvert.SerializeObject(gameUserDto);

                // Create StringContent from JSON
                var content = new StringContent(jsonContent);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string endpoint = $"/Game/addGameForUser";

                // Make API call with the JSON content in the request body
                var response = await _apiService.MakeApiCall(endpoint, HttpMethod.Post, content);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert($"{_selectedGame.Name} tilføjet", $"{_selectedGame.Name} tilføjet til bruger {username}", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Fejl", $"Kunne ikke tilføje spil", "OK");

                }
            }
            else
            {
                await Shell.Current.DisplayAlert($"Fejl", $"{username} har allerede dette spil", "OK");

            }


        }
        [RelayCommand]
        private async Task SelectGame(Game selectedGame)
        {
            _selectedGame = selectedGame;
            IsAddButtonVisible = !IsAddButtonVisible;

        }

        #endregion


    }
}