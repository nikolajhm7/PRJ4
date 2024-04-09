using Microsoft.AspNetCore.Mvc;
using Serilog;
using Server.API.DTO;

namespace Server.API.Controllers;
public class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogger<LogsController> logger)
    {
        _logger = logger;
    }

    [HttpPost("logs")]
    public IActionResult Logs([FromBody] LogEntryDTO logEntry)
    {
        _logger.LogInformation("Received log entry: {LogEntry}", logEntry);
        if (logEntry == null)
        {
            return BadRequest("Log entry cannot be null");
        }

        var logMessage = $"[Client] {logEntry.Message}";
        
        switch (logEntry.Level.ToLower())
        {
            case "debug":
                _logger.LogDebug(logMessage);
                break;
            case "info":
                _logger.LogInformation(logMessage);
                break;
            case "warn":
                _logger.LogWarning(logMessage);
                break;
            case "error":
                _logger.LogError(logMessage);
                break;
            case "critical":
                _logger.LogCritical(logMessage);
                break;
            default:
                _logger.LogInformation(logMessage); // Standard til info, hvis ikke specificeret
                break;
        }

        return Ok();
    }
}