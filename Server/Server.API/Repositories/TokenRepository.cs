using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repository.Interfaces;

namespace Server.API.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly ApplicationDbContext _context;

    public TokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void SaveRefreshToken(string username, string refreshToken, DateTime expiryDate)
    {
        var user = _context.Users.Include(u => u.RefreshTokens).FirstOrDefault(u => u.UserName == username);
        if (user != null)
        {
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expires = expiryDate,
                Created = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }

    public string GetRefreshToken(string username)
    {
        var user = _context.Users.Include(u => u.RefreshTokens).FirstOrDefault(u => u.UserName == username);
        return user?.RefreshTokens.LastOrDefault()?.Token;
    }

    public bool IsActive(string username)
    {
        var user = _context.Users.Include(u => u.RefreshTokens).FirstOrDefault(u => u.UserName == username);
        return user?.RefreshTokens.LastOrDefault()?.IsActive ?? false;
    }
}
