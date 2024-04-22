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


namespace Client.UI.Services
{
    public abstract class ConnectionService
    {
        public record ActionResult(bool Success, string? Msg);
        public record ActionResult<T>(bool Success, string? Msg, T? Value);

        private readonly HubConnection _hubConnection;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ConnectionService(string url)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => {
                        var json = Preferences.Get("auth_token", defaultValue: "{}");
                        var token = JObject.Parse(json)["token"]?.ToString();
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

        public async Task<ActionResult> InvokeAsync(string methodName, params object[] args)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult>(methodName, args);
            }
            else
            {
                return new ActionResult(false, "No connection to server.");
            }
        }

        public async Task<ActionResult<T>> InvokeAsync<T>(string methodName, params object[] args)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<ActionResult<T>>(methodName, args);
            }
            else
            {   
                ActionResult<T> actionResult = new(false, "No connection to server.", default(T));
                return actionResult;
            }
        }

        public void On<T>(string methodName, Action<T> handler)
        {
            _hubConnection.On(methodName, handler);
        }

        public void On(string methodName, Action handler)
        {
            _hubConnection.On(methodName, handler);
        }
    }
}
