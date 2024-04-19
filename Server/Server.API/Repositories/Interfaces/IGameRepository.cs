using Server.API.Models;

namespace Server.API.Repository.Interfaces;

public interface IGameRepository
{
    public Task<List<Game>> GetGamesForUser(User user);
}