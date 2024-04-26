namespace Client.Library.Services;

public interface IJwtTokenService
{
    public Task<bool> IsAuthenticated();
    public string GetUsernameFromToken();
    
    public bool SetTokensFromResponse(HttpResponseMessage response);
}