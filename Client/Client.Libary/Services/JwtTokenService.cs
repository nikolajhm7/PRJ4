using Client.Libary.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Client.Libary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Client.Libary.Services;

public class JwtTokenService : IJwtTokenService
{
    public IApiService _apiService;
    public JwtTokenService(IApiService apiService)
    {
        _apiService = apiService;
    }
    
    public async Task<bool> IsAuthenticated()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var result = await _apiService.MakeApiCall("/checkLoginToken", HttpMethod.Post);
           if (result.StatusCode == HttpStatusCode.OK)
           {
               return true;
           }
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