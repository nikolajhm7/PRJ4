namespace Client.Libary.Interfaces;

public interface IJwtTokenService
{
    bool IsAuthenticated();
    string GetUsernameFromToken();
}