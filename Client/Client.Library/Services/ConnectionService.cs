using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Client.Library.Models;


namespace Client.Library.Services
{
    public abstract class ConnectionService : IConnectionService
    {

        private readonly HubConnection _hubConnection;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ConnectionService(string url)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => {
                        var token = Preferences.Get("auth_token", defaultValue: "{}");
                        Debug.WriteLine($"Using token: {token}");
                        return Task.FromResult(token);
                    };
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddDebug();
                })
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (!IsConnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it accordingly
                    Debug.WriteLine($"Error while trying to start connection: {ex.Message}");
                    throw;  // Rethrow or handle as necessary
                }
            }
        }

        public async Task DisconnectAsync()
        {
            if (IsConnected)
            {
                await _hubConnection.StopAsync();
            }
        }

        public async Task<ActionResult> InvokeAsync(string methodName)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult>(methodName);
            }
            else
            {
                return new ActionResult(false, "No connection to server.");
            }
        }

        public async Task<ActionResult> InvokeAsync(string methodName, object? arg1)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult>(methodName, arg1);
            }
            else
            {
                return new ActionResult(false, "No connection to server.");
            }
        }

        public async Task<ActionResult> InvokeAsync(string methodName, object? arg1, object? arg2)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult>(methodName, arg1, arg2);
            }
            else
            {
                return new ActionResult(false, "No connection to server.");
            }
        }

        public async Task<ActionResult> InvokeAsync(string methodName, object? arg1, object? arg2, object? arg3)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult>(methodName, arg1, arg2, arg3);
            }
            else
            {
                return new ActionResult(false, "No connection to server.");
            }
        }

        public async Task<ActionResult<T>> InvokeAsync<T>(string methodName, object? arg1)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult<T>>(methodName, arg1);
            }
            else
            {   
                ActionResult<T> actionResult = new(false, "No connection to server.", default(T));
                return actionResult;
            }
        }

        public void On(string methodName, Action handler)
        {
            _hubConnection.On(methodName, handler);
        }

        public void On<T>(string methodName, Action<T> handler)
        {
            _hubConnection.On(methodName, handler);
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> handler)
        {
            _hubConnection.On(methodName, handler);
        }

        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
        {
            _hubConnection.On(methodName, handler);
        }
    }
}
