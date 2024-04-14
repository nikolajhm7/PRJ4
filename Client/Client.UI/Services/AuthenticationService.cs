using Microsoft.Extensions.Configuration;

namespace Client.UI.Services;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthenticationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        var token = Preferences.Get("auth_token", defaultValue: string.Empty);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration["ConnectionSettings:ApiUrl"] + "/checkLoginToken");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
            return response.IsSuccessStatusCode;
        }
        return false;
    }
}