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
        Task<ActionResult> InvokeAsync(string methodName, object? arg1);
        Task<ActionResult> InvokeAsync(string methodName, object? arg1, object? arg2);
        Task<ActionResult<T>> InvokeAsync<T>(string methodName, object? arg1);
        void On(string methodName, Action handler);
        void On<T>(string methodName, Action<T> handler);
        void On<T1, T2>(string methodName, Action<T1, T2> handler);
        void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler);
    }
}
