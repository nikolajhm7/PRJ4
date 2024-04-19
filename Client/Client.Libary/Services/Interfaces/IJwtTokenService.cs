namespace Client.Libary.Interfaces;

public interface IJwtTokenService
{
    public Task<bool> IsAuthenticated();
    string GetUsernameFromToken();
}