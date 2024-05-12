using Server.API.Services.Interfaces;

namespace Server.API.Services;

public class TimeService : ITimeService
{
    private TimeSpan _timeOffset = TimeSpan.Zero;

    public DateTime Now => DateTime.Now.Add(_timeOffset);
    public DateTime UtcNow => DateTime.UtcNow.Add(_timeOffset);
    
    // Metode til at fremskynde tiden
    public void AdvanceTime(int minutes)
    {
        _timeOffset = _timeOffset.Add(TimeSpan.FromMinutes(minutes));
    }

    // Metode til at nulstille tiden (kan være nyttig i test)
    public void ResetTime()
    {
        _timeOffset = TimeSpan.Zero;
    }
}