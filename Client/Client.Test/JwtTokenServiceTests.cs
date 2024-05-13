using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Client.Library.Interfaces;
using Client.Library.Services;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NSubstitute;

namespace Client.Test;

public class JwtTokenServiceTests
{
    private IApiService _apiService;
    private IPreferenceManager _preferenceManager;
    private JwtTokenService _jwtTokenService;

    [SetUp]
    public void Setup()
    {
        _apiService = Substitute.For<IApiService>();
        _preferenceManager = Substitute.For<IPreferenceManager>();
        _jwtTokenService = new JwtTokenService(_apiService, _preferenceManager);
    }
    
    [Test]
    public async Task IsAuthenticated_ReturnsTrue_WhenTokenIsNotEmptyAndApiCallReturnsOk()
    {
        _preferenceManager.Get("auth_token", Arg.Any<string>()).Returns("valid_token");
        _apiService.MakeApiCall("/checkLoginToken", HttpMethod.Post).Returns(new HttpResponseMessage(HttpStatusCode.OK));
        
        // Act
        var result = await _jwtTokenService.IsAuthenticated();
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public async Task IsAuthenticated_ReturnsFalse_WhenTokenIsNotEmptyAndApiCallReturnsNotOk()
    {
        _preferenceManager.Get("auth_token", Arg.Any<string>()).Returns("valid_token");
        _apiService.MakeApiCall("/checkLoginToken", HttpMethod.Post).Returns(new HttpResponseMessage(HttpStatusCode.BadRequest));
        
        // Act
        var result = await _jwtTokenService.IsAuthenticated();
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GetUsernameFromToken_ShouldReturnUsername_WhenTokenIsValid()
    {
        // Arrange
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyVerySecureKeyMyVerySecureKeyMyVerySecureKey"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "johndoe")
        };

        var token = new JwtSecurityToken(
            issuer: "YourIssuer",
            audience: "YourAudience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();
        var serializedToken = handler.WriteToken(token);

        var mockPreferenceManager = Substitute.For<IPreferenceManager>();
        var jwtTokenService = new JwtTokenService(null, mockPreferenceManager);

        mockPreferenceManager.Get("auth_token", Arg.Any<string>()).Returns(serializedToken);

        // Act
        var username = jwtTokenService.GetUsernameFromToken();

        // Assert
        Assert.That(username, Is.EqualTo("johndoe"));
    }
    
    [Test]
    public void SetTokensFromResponse_ShouldSetTokens_WhenResponseIsValid()
    {
        var jsonContent = "{\"token\": \"new_jwt_token\", \"refreshToken\": \"new_refresh_token\"}";
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        
        _apiService.GetJsonObjectFromResponse(Arg.Any<HttpResponseMessage>()).Returns(JObject.Parse(jsonContent));

        // Act
        var result = _jwtTokenService.SetTokensFromResponse(response);

        // Assert
        Assert.That(result, Is.True);
        _preferenceManager.Received().Set("auth_token", "new_jwt_token");
        _preferenceManager.Received().Set("refresh_token", "new_refresh_token");
    }
    
    [Test]
    public void SetTokensFromResponse_ShouldNotSetTokens_WhenResponseIsNotValid()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        
        // Act
        var result = _jwtTokenService.SetTokensFromResponse(response);

        // Assert
        Assert.That(result, Is.False);
        _preferenceManager.DidNotReceive().Set("auth_token", Arg.Any<string>());
        _preferenceManager.DidNotReceive().Set("refresh_token", Arg.Any<string>());
    }
    
    [Test]
    public void IsUserRoleGuest_ShouldReturnTrue_WhenUserRoleIsGuest()
    {
        // Arrange
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyVerySecureKeyMyVerySecureKeyMyVerySecureKey"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Guest")
        };

        var token = new JwtSecurityToken(
            issuer: "YourIssuer",
            audience: "YourAudience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();
        var serializedToken = handler.WriteToken(token);

        var mockPreferenceManager = Substitute.For<IPreferenceManager>();
        mockPreferenceManager.Get("auth_token", Arg.Any<string>()).Returns(serializedToken);

        var jwtTokenService = new JwtTokenService(null, mockPreferenceManager);

        // Act
        var isGuest = jwtTokenService.IsUserRoleGuest();

        // Assert
        Assert.That(isGuest, Is.True);
    }




}