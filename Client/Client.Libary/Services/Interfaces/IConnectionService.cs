using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Libary.Models;

namespace Client.Libary.Services
{
    public interface IConnectionService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task<ActionResult> InvokeAsync(string methodName, params object[] args);
        Task<ActionResult<T>> InvokeAsync<T>(string methodName, params object[] args);
        void On<T>(string methodName, Action<T> handler);
        void On(string methodName, Action handler);
    }
}
