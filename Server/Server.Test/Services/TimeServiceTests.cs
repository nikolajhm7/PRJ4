using Server.API.Services;

namespace Server.Test.Services;

public class TimeServiceTests
{
    
    [SetUp]
    public void SetUp()
    {
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


}