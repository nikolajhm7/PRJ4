namespace Server.API.Services.Interfaces;

public interface ITimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    
    void AdvanceTime(int minutes);
    void ResetTime();
}
