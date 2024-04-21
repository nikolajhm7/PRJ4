using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Middleware;

namespace Server.Test.Middleware;

public class ErrorLoggingMiddlewareTests
{
    private RequestDelegate _next;
    private ILogger<ErrorLoggingMiddleware> _logger;
    private ErrorLoggingMiddleware _middleware;

    [SetUp]
    public void Setup()
    {
        // Mock the next delegate in the middleware pipeline
        _next = Substitute.For<RequestDelegate>();

        // Mock the logger
        _logger = Substitute.For<ILogger<ErrorLoggingMiddleware>>();

        // Instantiate the middleware with the mocked dependencies
        _middleware = new ErrorLoggingMiddleware(_next, _logger);
    }
    
    [Test]
    public async Task Invoke_WhenExceptionOccurs_ShouldLogErrorAndReturnInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Set up to capture response body

        var exception = new Exception("Test exception");
        _next.When(x => x(Arg.Any<HttpContext>())).Do(x => throw exception);

        // Act
        await _middleware.Invoke(context);

        // Assert - Ensure that the logger was called with the expected exception
        _logger.Received().LogError(exception, "En uventet fejl opstod for en request.");

        // Assert - Check the response status code and content type
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));

        // Read the response output to verify the correct error message was returned
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var expectedJson = "{\"error\":\"En intern serverfejl opstod.\"}";
        Assert.That(responseBody, Is.EqualTo(expectedJson));
    }
}