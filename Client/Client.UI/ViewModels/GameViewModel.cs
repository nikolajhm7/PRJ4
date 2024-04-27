using Client.Library.Services;
using Client.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Client.Library.Games;
using System.Windows.Input;
using Microsoft.Maui.Controls;
namespace Client.UI.ViewModels
{
    [QueryProperty(nameof(LobbyId), "LobbyId")]
    public partial class GameViewModel : ObservableObject
    {
        private readonly IHangmanService _hangmanService;

        public GameViewModel(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;
            guessedChars = new ObservableCollection<char>();

            //_hangmanService.GameStartedEvent += OnGameStarted;
            //_hangmanService.GuessResultEvent += OnGuessResult;
            //_hangmanService.GameOverEvent += OnGameOver;

        }

        // Define command properties
        public string Title { get; }
        public int Players { get; }
        [ObservableProperty] private int errorCounter;
        [ObservableProperty] private string? lobbyId;
        [ObservableProperty] private char? letter;
        ObservableCollection<char> guessedChars;

        public ObservableCollection<char> GuessedChars
        {
            get { return guessedChars; }
            set { SetProperty(ref guessedChars, value); }
        }
        public void OnPageAppearing()
        {
            StartGame();
        }

        private void OnGameStarted(int wordlength)
        {
            Console.WriteLine($"Game started with wordlength {wordlength}");
        }

        [RelayCommand]
        private async Task StartGame()
        {
            try
            {
                await _hangmanService.StartGame(LobbyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
            }
        }
        [RelayCommand]
        private void GuessLetter(char letter)
        {
            guessedChars.Add(letter);
            //try
            //{
            //    await _hangmanService.GuessLetter(LobbyId, letter);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error guessing letter: {ex.Message}");
            //}
        }

        [RelayCommand]
        private async Task RestartGame()
        {
            try
            {
                await _hangmanService.RestartGame(LobbyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting game: {ex.Message}");
            }
        }

    }
}

