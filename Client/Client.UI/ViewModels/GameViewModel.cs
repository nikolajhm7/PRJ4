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

namespace Client.UI.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly HubConnection _connection;

        [ObservableProperty]
        string _name;

        [ObservableProperty]
        string _message;

        [ObservableProperty]
        ObservableCollection<string> _messages;

        [ObservableProperty]
        bool _isConnected;

        public GameViewModel()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5008/HangmanGame")
                .Build();

            _connection.On<string>("InvalidCategory", (message) =>
            {
                Console.WriteLine("Invalid category selected.");
            });

            _connection.On<int>("GameStarted", (wordLength) =>
            {
                Console.WriteLine($"Game started. Word length: {wordLength}");
            });

            _connection.On<char, bool>("GuessResult", (letter, isCorrect) =>
            {
                Console.WriteLine($"Guess result: Letter '{letter}', Correct: {isCorrect}");
            });


            //_connection.On<int>("GameStarted", (message) => 
            //{
            //    for (int i = 0; i < message; i++)
            //    {
            //        secretWord.Text += "_ ";
            //    }
            //});

            _connection.On<bool, string>("GameOver", (isWin, secretWord) =>
            {
                if (isWin)
                {
                    Console.WriteLine($"You won! The word was: {secretWord}");
                }
                else
                {
                    Console.WriteLine($"Game over! The word was: {secretWord}");
                }
            });
        }

        private async void StartGame(object sender, EventArgs e)
        {
            string category = "Animals";

            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("StartGame", category);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
            }
        }

        public async Task GuessLetter(char letter)
        {
            try
            {
                await _connection.InvokeAsync("MakeMove", letter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making move: {ex.Message}");
            }
        }

        //private async void OnStartClicked(object sender, EventArgs e)
        //{
        //    await _connection.InvokeCoreAsync("StartGame", args: new[]
        //    {});
        //}
    }
}
