using Client.Libary.Interfaces;
using System.IdentityModel.Tokens.Jwt;
namespace Client.Libary;

public class JwtTokenService : IJwtTokenService
{
    public JwtTokenService()
    {
    }
    
    public bool IsAuthenticated()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);

        if (!string.IsNullOrWhiteSpace(token))
        {
            return true;
        }
        return false;
    }
    
    public string GetUsernameFromToken()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        return jsonToken.Claims.First(claim => claim.Type == "unique_name").Value;
    }
    
}