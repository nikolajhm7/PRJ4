using NSubstitute;
using System.Net;
using System.Text;
using Client.Library;
using Client.Library.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSubstitute.ExceptionExtensions;

namespace Client.Test;
public class ApiServiceTests
{
    private ApiService _apiService;
    private HttpClient _client;
    private IConfiguration _configuration;
    private ILogger<ApiService> _logger;
    private FakeHttpMessageHandler _handler;
    private IPreferenceManager _preferenceManager;

    [SetUp]
    public void Setup()
    {
        _handler = new FakeHttpMessageHandler();
        _client = new HttpClient(_handler);
        _configuration = Substitute.For<IConfiguration>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(_client);
        _logger = Substitute.For<ILogger<ApiService>>();

        _configuration["ConnectionSettings:ApiUrl"].Returns("http://example.com/api/");
        
        _preferenceManager = Substitute.For<IPreferenceManager>();

        _apiService = new ApiService(_configuration, _logger, httpClientFactory, _preferenceManager);
    }

    [Test]
    public async Task MakeApiCall_GetMethod_Success()
    {
        var response = await _apiService.MakeApiCall("/test", HttpMethod.Get);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task MakeApiCall_SendsCorrectAuthHeaders()
    {
        // Sæt mock data for tokens
        _preferenceManager.ContainsKey("auth_token").Returns(true);
        _preferenceManager.Get("auth_token", "").Returns("valid_auth_token");
        _preferenceManager.ContainsKey("refresh_token").Returns(true);
        _preferenceManager.Get("refresh_token", "").Returns("valid_refresh_token");

        // Udfør API kald
        await _apiService.MakeApiCall("/test", HttpMethod.Get);

        // Kontroller at de korrekte headers er blevet sat
        Assert.That(_handler.LastRequest.Headers.Authorization?.Parameter, Is.EqualTo("valid_auth_token"));
        Assert.That(_handler.LastRequest.Headers.Contains("X-Refresh-Token"), Is.True);
        Assert.That(_handler.LastRequest.Headers.GetValues("X-Refresh-Token").First(), Is.EqualTo("valid_refresh_token"));
    }

    [Test]
    public async Task MakeApiCall_UpdatesTokensOnReceivingNewHeaders()
    {
        // Opsæt en respons med nye tokens
        var newTokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Headers =
            {
                { "X-New-AccessToken", "new_access_token" },
                { "X-New-RefreshToken", "new_refresh_token" }
            }
        };
        _handler.SetFakeResponse(newTokenResponse);

        // Udfør API kald
        await _apiService.MakeApiCall("/test", HttpMethod.Get);

        // Verificer at de nye tokens er blevet opdateret korrekt
        _preferenceManager.Received().Set("auth_token", "new_access_token");
        _preferenceManager.Received().Set("refresh_token", "new_refresh_token");
    }
    
    [Test]
    public async Task MakeApiCall_PostMethod_Success()
    {
        var someObject = new { name = "test" };
        var json = JsonConvert.SerializeObject(someObject);
        // Opsæt data til POST-anmodningen
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Opsæt en forventet respons
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _handler.SetFakeResponse(expectedResponse);

        // Udfør API kald
        var response = await _apiService.MakeApiCall("/posttest", HttpMethod.Post, content);

        // Kontroller at metoden er POST
        Assert.That(_handler.LastRequest.Method, Is.EqualTo(HttpMethod.Post));

        // Kontroller at indholdet blev sendt korrekt
        var requestContent = await _handler.LastRequest.Content.ReadAsStringAsync();
        Assert.That(requestContent, Is.EqualTo("{\"name\":\"test\"}"));

        // Kontroller at statuskode for respons er som forventet
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task MakeApiCall_PutMethod_Success()
    {
        var someObject = new { name = "test" };
        var json = JsonConvert.SerializeObject(someObject);
        // Opsæt data til PUT-anmodningen
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Opsæt en forventet respons
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _handler.SetFakeResponse(expectedResponse);

        // Udfør API kald
        var response = await _apiService.MakeApiCall("/puttest", HttpMethod.Put, content);

        // Kontroller at metoden er PUT
        Assert.That(_handler.LastRequest.Method, Is.EqualTo(HttpMethod.Put));

        // Kontroller at indholdet blev sendt korrekt
        var requestContent = await _handler.LastRequest.Content.ReadAsStringAsync();
        Assert.That(requestContent, Is.EqualTo("{\"name\":\"test\"}"));

        // Kontroller at statuskode for respons er som forventet
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task MakeApiCall_DeleteMethod_Success()
    {
        // Opsæt en forventet respons
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _handler.SetFakeResponse(expectedResponse);

        // Udfør API kald
        var response = await _apiService.MakeApiCall("/deletetest", HttpMethod.Delete);

        // Kontroller at metoden er DELETE
        Assert.That(_handler.LastRequest.Method, Is.EqualTo(HttpMethod.Delete));

        // Kontroller at statuskode for respons er som forventet
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task MakeApiCall_ReturnsInternalServerError_ForUnsupportedMethod()
    {
        // Arrange
        var endpoint = "https://example.com/api/test";
        var unsupportedMethod = new HttpMethod("PATCH"); // En metode, der ikke er understøttet
        var content = new StringContent("{\"data\":\"test\"}");

        // Act
        var response = await _apiService.MakeApiCall(endpoint, unsupportedMethod, content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("Internal server error: Unsupported HTTP method"));
    }

    [Test]
    public async Task MakeApiCall_LogsError_WhenApiCallFails()
    {
        // Arrange
        var endpoint = "https://example.com/api/test";
        var method = HttpMethod.Get;
        var content = new StringContent("{\"data\":\"test\"}");

        // Opsætter et fejlrespons
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request error")
        };
        
        _handler.SetFakeResponse(response);
        
        _client = new HttpClient(_handler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(_client);
        _apiService = new ApiService(_configuration, _logger, httpClientFactory, _preferenceManager);
        
        // Act
        await _apiService.MakeApiCall(endpoint, method, content);

        // Assert
        _logger.Received().LogError($"Error calling url: {endpoint}. Status code: {response.StatusCode}");
        
        
    }
    
    [Test]
    public async Task GetJsonObjectFromResponse_Success()
    {
        // Opsæt en forventet respons
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json")
        };
        _handler.SetFakeResponse(expectedResponse);

        // Udfør API kald
        var response = await _apiService.MakeApiCall("/test", HttpMethod.Get);

        // Hent JSON objekt fra respons
        var result = _apiService.GetJsonObjectFromResponse(response);

        // Kontroller at objektet blev hentet korrekt
        Assert.That(result["name"].ToString(), Is.EqualTo("test"));
    }


    
    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _handler.Dispose();
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage LastRequest { get; private set; }
    private HttpResponseMessage _fakeResponse;

    // Tilføj en metode til at konfigurere det simulerede respons
    public void SetFakeResponse(HttpResponseMessage response)
    {
        _fakeResponse = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request; // Gemmer den seneste request for inspektion i testene
        return Task.FromResult(_fakeResponse ?? new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("No response configured")
        });
    }
    
    
}
