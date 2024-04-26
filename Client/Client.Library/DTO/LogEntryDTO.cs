using System;

namespace Client.Library.DTO;

public class LogEntryDTO
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}
