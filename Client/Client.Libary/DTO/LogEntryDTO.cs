using System;

namespace Client.Libary.DTO;

public class LogEntryDTO
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}
