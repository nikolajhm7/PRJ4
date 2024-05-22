using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Server.API.Controllers;
using Server.API.DTO;

namespace Server.Test;

public class LogsControllerTests
{
    private LogsController _controller;
    private ILogger<LogsController> _logger;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<LogsController>>();
        _controller = new LogsController(_logger)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
    
    [Test]
    public void Logs_ReturnsBadRequest_WhenLogEntriesAreNull()
    {
        var result = _controller.Logs(null) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("Log entry cannot be null or empty"));
        _logger.Received(1).LogInformation("Ingen logindgange modtaget.");
    }

    [Test]
    public void Logs_ProcessesEntries_AndLogsAppropriately()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "info", Message = "Test info", Timestamp = DateTime.UtcNow },
            new LogEntryDTO { Level = "warn", Message = "Test warning", Timestamp = DateTime.UtcNow }
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        Received.InOrder(() =>
        {
            _logger.Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test info")), null, Arg.Any<Func<object, Exception, string>>());
            _logger.Log(LogLevel.Warning, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test warning")), null, Arg.Any<Func<object, Exception, string>>());
        });
    }

    [Test]
    public void Logs_ProcessesEntriesAbbreviated_AndLogsAppropriately()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "dbg", Message = "Test debug", Timestamp = DateTime.UtcNow },
            new LogEntryDTO { Level = "inf", Message = "Test info", Timestamp = DateTime.UtcNow },
            new LogEntryDTO { Level = "err", Message = "Test error", Timestamp = DateTime.UtcNow },
            new LogEntryDTO { Level = "crit", Message = "Test critical", Timestamp = DateTime.UtcNow },
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        Received.InOrder(() =>
        {
            _logger.Log(LogLevel.Debug, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test debug")), null, Arg.Any<Func<object, Exception, string>>());
            _logger.Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test info")), null, Arg.Any<Func<object, Exception, string>>());
            _logger.Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test error")), null, Arg.Any<Func<object, Exception, string>>());
            _logger.Log(LogLevel.Critical, Arg.Any<EventId>(), Arg.Is<object>(o => o.ToString().Contains("Test critical")), null, Arg.Any<Func<object, Exception, string>>());
        });
    }


    [Test]
    public void Logs_HandlesUnknownLogLevel()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "unknown", Message = "Unknown level", Timestamp = DateTime.UtcNow }
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        // Her verificeres blot, at der er foretaget et log kald med niveauet Information.
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }

    [Test]
    public void Logs_HandlesDebugLevel()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "debug", Message = "Debug message", Timestamp = DateTime.UtcNow }
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
        
    }

    [Test]
    public void Logs_HandlesErrorLevel()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "error", Message = "Error message", Timestamp = DateTime.UtcNow }
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }

    [Test]
    public void Logs_HandlesCriticalLevel()
    {
        var logEntries = new List<LogEntryDTO>
        {
            new LogEntryDTO { Level = "critical", Message = "Critical message", Timestamp = DateTime.UtcNow }
        };

        var result = _controller.Logs(logEntries) as OkResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
        _logger.Received().Log(
            LogLevel.Critical,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }


}
