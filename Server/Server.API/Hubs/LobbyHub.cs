using Microsoft.AspNetCore.SignalR;
using Server.API.Models;
using Server.API.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Server.API.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        public record ActionResult(bool Result, string? Msg);

        private static readonly Dictionary<string, Lobby> lobbies = [];
        private static AsyncLocal<Random> randomLocal = new AsyncLocal<Random>(); // Tilfældig lobby ID

        public static async Task<string> GenerateRandomLobbyIDAsync()
        {
            Random random = GetThreadRandom();
            await Task.Delay(100); // Simulate some asynchronous operation
            int randomNumber = random.Next(0, 999999);
            return randomNumber.ToString("D6"); // to make the correct format
        }
        // Support function for GenerateRandomLobbyIDAsync
        private static Random GetThreadRandom()
        {
            if (randomLocal.Value == null)
            {
                randomLocal.Value = new Random(); // Create a new Random instance for this thread
            }
            return randomLocal.Value;
        }

        public async Task<ActionResult> CreateLobby()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var lobbyId = await GenerateRandomLobbyIDAsync();
                var lobby = new Lobby(lobbyId, Context.ConnectionId);

                lobby.Members.Add(new ConnectedUserDTO(Context.User.Identity.Name ?? "Guest", Context.ConnectionId));
                lobbies.Add(lobbyId, lobby);

                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                return new ActionResult(true, lobbyId);
            }
            else
            {
                return new ActionResult(false, "User not authenticated.");
            }
        }

        public async Task<ActionResult> JoinLobby(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                if (Context.User?.Identity?.IsAuthenticated == true)
                {
                    var username = Context.User.Identity.Name ?? "Guest";
                    var user = new ConnectedUserDTO(username, Context.ConnectionId);

                    lobby.Members.Add(user);
                    await Clients.Group(lobbyId).SendAsync("UserJoinedLobby", user);
                    await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                    return new ActionResult(true, null);
                }
                else
                {
                    return new ActionResult(false, "User not authenticated.");
                }
            }
            else
            {
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        public async Task<ActionResult> LeaveLobby(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                if (Context.User?.Identity?.IsAuthenticated == true)
                {
                    var username = Context.User.Identity.Name ?? "Guest";
                    var user = new ConnectedUserDTO(username, Context.ConnectionId);

                    lobby.Members.Remove(user);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                    await Clients.Group(lobbyId).SendAsync("UserLeftLobby", user);
                    return new ActionResult(true, null);
                }
                else
                {
                    return new ActionResult(false, "User not authenticated.");
                }
            }
            else
            {
                return new ActionResult(false, "Lobby does not exist.");
            }
        }

        public async Task<ActionResult> StartGame(string lobbyId)
        {
            if (lobbies.TryGetValue(lobbyId, out Lobby? lobby) && lobby.HostConnectionId == Context.ConnectionId)
            {
                // TODO: ADD START GAME LOGIC HERE

                await Clients.Group(lobbyId).SendAsync("GameStarted");

                // TODO: ADD GAME LOGIC CONTINUATION HERE

                return new ActionResult(true, null);
            }
            return new ActionResult(false, "Lobby does not exist.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name ?? "Guest";
            var user = new ConnectedUserDTO(username, Context.ConnectionId);

            var lobbyId = lobbies.FirstOrDefault(x => x.Value.Members.Contains(user)).Key;
            if (lobbyId != null)
            {
                await LeaveLobby(lobbyId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}