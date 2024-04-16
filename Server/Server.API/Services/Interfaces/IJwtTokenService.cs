namespace Server.API.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string userName, bool isGuest = false);
    string GenerateRefreshToken(string userName);
    bool ValidateRefreshToken(string userName, string refreshToken);
    string GetUserNameFromToken(string token);
    bool IsGuest(string token);
}