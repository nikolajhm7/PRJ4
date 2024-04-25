using Server.API.DTO;
using Server.API.Models;

namespace Server.API.Services.Interfaces
{
    public interface ILobbyManager
    {
        bool LobbyExists(string lobbyId);
        bool IsHost(string connectionId, string lobbyId);
        string? GetLobbyIdFromUser(ConnectedUserDTO user);
        List<ConnectedUserDTO> GetUsersInLobby(string lobbyId);
        string CreateNewLobby(ConnectedUserDTO user, int gameId);
        ActionResult AddToLobby(ConnectedUserDTO user, string lobbyId);
        void RemoveFromLobby(ConnectedUserDTO user, string lobbyId);
        void RemoveLobby(string lobbyId);
        void StartGame(string lobbyId);
        GameStatus GetGameStatus(string lobbyId);
    }
}
