using Server.API.Models;

namespace Server.API.Repository.Interfaces;

public interface IGameRepository
{
    public Task<List<Game>> GetGamesForUser(User user);
    public Task AddGameToUser(string userId, int gameId);
    public Task AddGame(Game game);
    public Task<Game> GetGameById(int gameId);
    public Task EditGame(Game game);
    public Task<int> GetMaxPlayers(int gameId);
}