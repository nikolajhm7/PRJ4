namespace Client.Libary.Services;

public interface IJwtTokenService
{
    public Task<bool> IsAuthenticated();
    string GetUsernameFromToken();
}