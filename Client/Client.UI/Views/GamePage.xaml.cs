using System;
using Client.UI.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.UI.Views
{
    public partial class GamePage : ContentPage
    {
        private readonly HubConnection _connection;

        public GamePage(GameViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5008/hubs/hangmanhub")
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

            Task.Run(() =>
            {
                Dispatcher.Dispatch(async () =>
                await _connection.StartAsync());
                Console.WriteLine("Connection established.");
                
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