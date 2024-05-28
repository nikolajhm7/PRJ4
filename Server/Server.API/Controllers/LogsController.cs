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
                case "dbg": // Antager "DBG" for "debug", tilpas som nødvendigt
                case "debug":
                    _logger.LogDebug(logMessage);
                    break;
                case "inf": // Brug "INF" for "info"
                case "info":
                    _logger.LogInformation(logMessage);
                    break;
                case "warn": // "WARN" synes at være ens
                    _logger.LogWarning(logMessage);
                    break;
                case "err": // Antager "ERR" for "error", tilpas som nødvendigt
                case "error":
                    _logger.LogError(logMessage);
                    break;
                case "crit": // Antager "CRIT" for "critical", tilpas som nødvendigt
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