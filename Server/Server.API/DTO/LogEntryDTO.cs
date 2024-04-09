namespace Server.API.DTO;

public class LogEntryDTO
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}