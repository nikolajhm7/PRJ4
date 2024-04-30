namespace Server.API.Services.Interfaces
{
    public interface ILogicManager<T>
    {
        bool LobbyExists(string lobbyId);
        void Add(string lobbyId, T logic);
        bool TryGetValue(string lobbyId, out T logic);
        bool Remove(string lobbyId);
    }
}
