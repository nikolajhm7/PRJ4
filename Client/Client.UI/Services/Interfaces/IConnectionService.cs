using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Services
{
    public interface IConnectionService
    {
        public record ActionResult(bool Result, string? Msg);
        HubConnection GetConnection();
        Task ConnectAsync();
        Task DisconnectAsync();
        Task<ActionResult> InvokeAsync(string methodName, params object[] args);
        Task<T> InvokeAsync<T>(string methodName, params object[] args);
        void On<T>(string methodName, Action<T> handler);
        void On(string methodName, Action handler);
    }
}
