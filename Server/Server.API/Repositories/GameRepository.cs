using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.API.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;

    public GameRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Game>> GetGamesForUser(User user)
    {
        return await _context.Games
            .Where(g => g.UserGames.Any(ug => ug.Id == user.Id))
            .ToListAsync();
    }

}