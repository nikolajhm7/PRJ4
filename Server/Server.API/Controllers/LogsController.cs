using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpPost("logs")]
    public IActionResult Logs([FromBody] List<LogEntryDTO> logEntries)
    {
        if (logEntries == null || !logEntries.Any())
        {
            _logger.LogInformation("Ingen logindgange modtaget.");
            return BadRequest("Log entry cannot be null or empty");
        }

        foreach (var logEntry in logEntries)
        {

            var logMessage = $"[Client] [{logEntry.Timestamp:yy-MM-dd HH:mm:ss.fff}] {logEntry.Level.ToUpper()}: {logEntry.Message}";

            switch (logEntry.Level.ToLower())
            {
                case "dbg":
                case "debug":
                    _logger.LogDebug(logMessage);
                    break;
                case "inf":
                case "info":
                    _logger.LogInformation(logMessage);
                    break;
                case "warn":
                    _logger.LogWarning(logMessage);
                    break;
                case "err":
                case "error":
                    _logger.LogError(logMessage);
                    break;
                case "crit":
                case "critical":
                    _logger.LogCritical(logMessage);
                    break;
                default:
                    _logger.LogInformation("Ukendt logniveau '{Level}': {Message}", logEntry.Level, logMessage);
                    break;
            }
        }

        return Ok();
    }
}