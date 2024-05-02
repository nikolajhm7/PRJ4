using Server.API.DTO;
using Server.API.Models;

namespace Server.API.Services.Interfaces
{
    public interface ILobbyManager
    {
        bool LobbyExists(string lobbyId);
        bool IsHost(string connectionId, string lobbyId);
        void UpdateUserInLobby(ConnectedUserDTO newUser, string lobbyId);
        string? GetLobbyIdFromUsername(string username);
        List<ConnectedUserDTO> GetUsersInLobby(string lobbyId);
        Task<string> CreateNewLobby(ConnectedUserDTO user, int gameId);
        ActionResult<List<ConnectedUserDTO>> AddToLobby(ConnectedUserDTO user, string lobbyId);
        void RemoveFromLobby(ConnectedUserDTO user, string lobbyId);
        void RemoveLobby(string lobbyId);
        void StartGame(string lobbyId);
        GameStatus GetGameStatus(string lobbyId);
        ActionResult<int> GetLobbyGameId(string lobbyId);

    }
}
