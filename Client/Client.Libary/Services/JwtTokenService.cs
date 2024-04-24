using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Client.Libary.Interfaces;
using Client.Libary.Services;
using Client.Libary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Client.Libary.Services;

public class JwtTokenService : IJwtTokenService
{
    private IApiService _apiService;
    private IPreferenceManager _preferenceManager;

    public JwtTokenService(IApiService apiService, IPreferenceManager preferenceManager)
    {
        _apiService = apiService;
        _preferenceManager = preferenceManager;
    }

    public async Task<bool> IsAuthenticated()
    {
        var token = _preferenceManager.Get("auth_token", defaultValue: string.Empty);

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
        var token = _preferenceManager.Get("auth_token", defaultValue: string.Empty);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        return jsonToken.Claims.First(claim => claim.Type == "unique_name").Value;
    }

    private void GetTokensFromResponse(HttpResponseMessage response, out string jwtToken, out string refreshToken)
    {
        var jsonTokens = _apiService.GetJsonObjectFromResponse(response);
        jwtToken = jsonTokens["token"].ToString();
        refreshToken = jsonTokens["refreshToken"].ToString();
    }

    public bool SetTokensFromResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            GetTokensFromResponse(response, out var jwtToken, out var refreshToken);
            _preferenceManager.Set("auth_token", jwtToken);
            _preferenceManager.Set("refresh_token", refreshToken);
            return true;
        }

        return false;
    }
}