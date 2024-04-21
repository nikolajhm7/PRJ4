namespace Server.API.Repository.Interfaces;

public interface ITokenRepository
{
    void SaveRefreshToken(string userName, string refreshToken, DateTime expiryDate);
    string GetRefreshToken(string userName);
    bool IsActive(string userName);
}