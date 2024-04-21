using Server.API.Services.Interfaces;

namespace Server.API.Services;

public class TimeService : ITimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}