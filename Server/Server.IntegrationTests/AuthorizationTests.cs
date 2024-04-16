using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Server.API.Data;
using Server.API.Repository;
using Server.API.Services;
using Server.Test;

namespace Server.IntegrationTests;

public class AuthorizationTests : IntegrationTestBase
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
        var validToken = JwtTokenService.GenerateToken("testUser");
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkLoginToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validToken);

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task GetProtectedResourceWithGuestToken_ShouldReturnOk()
    {
        // Arrange
        var guestToken = JwtTokenService.GenerateToken("guestUser", isGuest: true);
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkGuestToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", guestToken);

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetProtectedResourceWithUserToken_ShouldReturnOkForGuestPolicy()
    {
        // Arrange
        var userToken = JwtTokenService.GenerateToken("testUser"); // isGuest er false som default
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkGuestToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetProtectedResourceWithoutToken_ShouldReturnUnauthorizedForGuestPolicy()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/checkGuestToken");

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
