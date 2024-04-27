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
    public partial class GameViewModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly IHangmanService _hangmanService;

        public GameViewModel(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;

            // Initialize commands
            GuessLetterCommand = new Command<char>(GuessLetter);


        }

        // Define command properties
        public ICommand GuessLetterCommand { get; }

        // Other properties and methods...

        [RelayCommand]
        private async Task StartGame() // Needs LobbyID as parameter, but cant be passed for now
        {
            string lobbyId = "YourLobbyId"; // Replace "YourLobbyId" with the actual lobby ID
            try
            {
                await _hangmanService.StartGame(lobbyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
            }
        }

        private async void GuessLetter(char letter)
        {
            string lobbyId = "YourLobbyId"; // Replace "YourLobbyId" with the actual lobby ID
            try
            {
                await _hangmanService.GuessLetter(lobbyId, letter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guessing letter: {ex.Message}");
            }
        }

    }
}

