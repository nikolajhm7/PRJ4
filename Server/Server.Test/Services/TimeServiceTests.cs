using Server.API.Services;

namespace Server.Test.Services;

public class TimeServiceTests
{
    private TimeService _timeService;

    [SetUp]
    public void SetUp()
    {
        _timeService = new TimeService();
    }
    
    [Test]
    public void Now_ReturnsCurrentLocalTime()
    {
        // Arrange
        var timeService = new TimeService();
        var before = DateTime.Now;

        // Act
        var result = timeService.Now;
        var after = DateTime.Now;

        // Assert
        Assert.That(result, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));
    }
    
    [Test]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        // Arrange
        var timeService = new TimeService();
        var before = DateTime.UtcNow;

        // Act
        var result = timeService.UtcNow;
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(result, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));
    }

    [Test]
    public void AdvanceTime_ReturnsNewUtcTime()
    {
        // Arrange
        var minutes = 30;
        var beforeNow = _timeService.Now;
        var beforeUtcNow = _timeService.UtcNow;

        // Act
        _timeService.AdvanceTime(minutes);

        // Assert
        Assert.That(_timeService.Now, Is.EqualTo(beforeNow.AddMinutes(minutes)).Within(10).Milliseconds);
        Assert.That(_timeService.UtcNow, Is.EqualTo(beforeUtcNow.AddMinutes(minutes)).Within(10).Milliseconds);
    }

    [Test]
    public void ResetTime_RestsNowAndUtcNowToCurrentTime()
    {
        // Arrange
        _timeService.AdvanceTime(30);
        var beforeNow = DateTime.Now;
        var beforeUtcNow = DateTime.UtcNow;

        // Act
        _timeService.ResetTime();

        // Assert
        Assert.That(_timeService.Now, Is.EqualTo(beforeNow).Within(10).Milliseconds);
        Assert.That(_timeService.UtcNow, Is.EqualTo(beforeUtcNow).Within(10).Milliseconds);
    }

}