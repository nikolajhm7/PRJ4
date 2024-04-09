using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.API.Services;

namespace Server.IntegrationTests;

public class AuthorizationTests
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task GetProtectedResourceWithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkLoginToken"); // Ændre til den rigtige URL

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetProtectedResourceWithValidToken_ShouldReturnOk()
    {
        // Arrange
        var jwtTokenService = new JwtTokenService(_factory.Services.GetRequiredService<IConfiguration>());
        var validToken = jwtTokenService.GenerateToken("testUser");
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkLoginToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }


    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
