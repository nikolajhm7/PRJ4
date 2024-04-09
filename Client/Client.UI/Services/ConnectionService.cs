using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Services
{
    public abstract class ConnectionService
    {
        public record ActionResult(bool Success, string? Msg);
        private readonly HubConnection _hubConnection;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ConnectionService(string url)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(Preferences.Get("auth_token", defaultValue: string.Empty));
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
                await _hubConnection.StartAsync();
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
