using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly HubConnection _hubConnection;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ConnectionService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(App.ApiUrl + "/socket")
                .Build();
        }

        public HubConnection GetConnection() => _hubConnection;

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

        public async Task<IConnectionService.ActionResult> InvokeAsync(string methodName, params object[] args)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await _hubConnection.InvokeAsync<IConnectionService.ActionResult>(methodName, args);
            }
            else
            {
                return new IConnectionService.ActionResult(false, "No connection to server.");
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
