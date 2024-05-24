using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.API.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;
    private IUserRepository _userRepository;

    public GameRepository(ApplicationDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }
    public async Task<List<Game>> GetAllGames()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<List<Game>> GetGamesForUser(User user)
    {
        return await _context.Games
            .Where(g => g.UserGames.Any(ug => ug.UserId == user.Id))
            .ToListAsync();
    }

    public async Task AddGameToUser(string username, int gameId)
    {
        var user = await _userRepository.GetUserByName(username);
        var game = await _context.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
        
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if (game == null)
        {
            throw new Exception("Game not found");
        }
        
        var userGame = new UserGame
        {
            UserId = user.Id,
            GameId = game.GameId
        };
        
        await _context.UserGames.AddAsync(userGame);
        await _context.SaveChangesAsync();
    }
    
    public async Task AddGame(Game game)
    {
        var existingGame = await _context.Games.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        
        if (existingGame != null)
        {
            throw new Exception("Game already exists");
        }
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
    }
    
    public async Task<int> GetMaxPlayers(int gameId)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
        
        if (game == null)
        {
            throw new Exception("Game not found");
        }
        
        return game.MaxPlayers;
    }
    
    public async Task<Game> GetGameById(int gameId)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
        
        if (game == null)
        {
            throw new Exception("Game not found");
        }
        
        return game;
    }
    
    public async Task EditGame(Game game)
    {
        var existingGame = await _context.Games.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        
        if (existingGame == null)
        {
            throw new Exception("Game not found");
        }
        
        existingGame.Name = game.Name;
        existingGame.MaxPlayers = game.MaxPlayers;
        
        await _context.SaveChangesAsync();
    }
}