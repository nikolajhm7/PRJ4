using Microsoft.Identity.Client;
using Server.API.Games;
using Server.API.Services.Interfaces;

namespace Server.API.Services
{
    public class LogicManager<T> : ILogicManager<T>
    {
        public readonly Dictionary<string, T> LobbyLogic = [];

        public bool LobbyExists(string lobbyId)
        {
            return LobbyLogic.ContainsKey(lobbyId);
        }

        public void Add(string lobbyId, T logic)
        {
            LobbyLogic.Add(lobbyId, logic);
        }

        public bool TryGetValue(string lobbyId, out T logic)
        {
            return LobbyLogic.TryGetValue(lobbyId, out logic);
        }

        public bool Remove(string lobbyId)
        {
            return LobbyLogic.Remove(lobbyId);
        }
    }
}
