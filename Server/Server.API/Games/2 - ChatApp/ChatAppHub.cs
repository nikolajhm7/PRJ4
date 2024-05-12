using Microsoft.AspNetCore.SignalR;

namespace Server.API.Games
{
    public class ChatAppHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

    }
}
