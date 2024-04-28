
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Client.Library.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Client.Library;

public class ApiService : IApiService
{
    private readonly HttpClient _client;
    private readonly ILogger<ApiService> _logger;
    private readonly IPreferenceManager _preferenceManager;

    public ApiService(IConfiguration configuration, ILogger<ApiService> logger, IHttpClientFactory httpClientFactory, IPreferenceManager preferenceManager)
    {
        _client = httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri(configuration["ConnectionSettings:ApiUrl"]);
        _logger = logger;
        _preferenceManager = preferenceManager;
    }

    public async Task<HttpResponseMessage> MakeApiCall(string endpoint, HttpMethod method, HttpContent content = null)
    {
        try
        {
            HttpResponseMessage response = null;

            if (_preferenceManager.ContainsKey("auth_token") && !string.IsNullOrEmpty(_preferenceManager.Get("auth_token", ""))
                                                             && _preferenceManager.ContainsKey("refresh_token") && !string.IsNullOrEmpty(_preferenceManager.Get("refresh_token", "")))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _preferenceManager.Get("auth_token", ""));
                _client.DefaultRequestHeaders.Add("X-Refresh-Token", _preferenceManager.Get("refresh_token", ""));
            }

            

            switch (method.Method)
            {
                case "GET":
                    response = await _client.GetAsync(endpoint);
                    break;
                case "POST":
                    response = await _client.PostAsync(endpoint, content);
                    break;
                case "PUT":
                    response = await _client.PutAsync(endpoint, content);
                    break;
                case "DELETE":
                    response = await _client.DeleteAsync(endpoint);
                    break;
                default:
                    throw new ArgumentException("Unsupported HTTP method");
            }

            if (response.IsSuccessStatusCode)
            {
                UpdateTokensIfProvided(response);
            }
            else
            {
                // Håndter fejl
                _logger.LogError($"Error calling url: {endpoint}. Status code: {response.StatusCode}");
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling url: {endpoint}", endpoint);
            // Skaber et nyt HttpResponseMessage objekt med en specifik statuskode
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Internal server error: {ex.Message}")
            };
        }
    }

    private void UpdateTokensIfProvided(HttpResponseMessage response)
    {
        if (response.Headers.Contains("X-New-AccessToken"))
        {
            var newAccessToken = response.Headers.GetValues("X-New-AccessToken").FirstOrDefault();
            _preferenceManager.Set("auth_token", newAccessToken);
        }

        if (response.Headers.Contains("X-New-RefreshToken"))
        {
            var newRefreshToken = response.Headers.GetValues("X-New-RefreshToken").FirstOrDefault();
            _preferenceManager.Set("refresh_token", newRefreshToken);
        }
    }

    public JObject GetJsonObjectFromResponse(HttpResponseMessage response)
    {
        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        return JObject.Parse(jsonResponse);
    }
}