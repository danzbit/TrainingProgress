using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Maps.v1;

namespace Bff.Application.Tests.Maps.v1;

[TestFixture]
public class SaveGoalSagaCommandToSaveGoalCommandMapperTests
{
    [Test]
    public void ToSaveGoalCommand_WithValidCommand_MapsAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 7, 23, 23, 59, 59, DateTimeKind.Utc);

        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg by summer",
            1,
            2,
            10,
            startDate,
            endDate,
            null);

        // Act
        var result = command.ToSaveGoalCommand(userId);

        // Assert
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Name, Is.EqualTo("Weight Loss"));
        Assert.That(result.Description, Is.EqualTo("Lose 10kg by summer"));
        Assert.That(result.MetricType, Is.EqualTo(1));
        Assert.That(result.PeriodType, Is.EqualTo(2));
        Assert.That(result.TargetValue, Is.EqualTo(10));
        Assert.That(result.StartDate, Is.EqualTo(startDate));
        Assert.That(result.EndDate, Is.EqualTo(endDate));
    }

    [Test]
    public void ToSaveGoalCommand_WithNullEndDate_MapsNullEndDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var command = new SaveGoalSagaCommand(
            "Running Distance",
            "Run 100km total",
            3,
            4,
            100,
            startDate,
            null,
            null);

        // Act
        var result = command.ToSaveGoalCommand(userId);

        // Assert
        Assert.That(result.EndDate, Is.Null);
    }

    [Test]
    public void ToSaveGoalCommand_WithDifferentMetricTypes_MapsDifferentMetrics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var commands = new[]
        {
            new SaveGoalSagaCommand("Distance", "Run 50km", 1, 1, 50, startDate, null, null),
            new SaveGoalSagaCommand("Weight", "Lose 5kg", 2, 1, 5, startDate, null, null),
            new SaveGoalSagaCommand("Calories", "Burn 10000 calories", 3, 1, 10000, startDate, null, null)
        };

        // Act
        var results = commands.Select(c => c.ToSaveGoalCommand(userId)).ToList();

        // Assert
        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results[0].MetricType, Is.EqualTo(1));
        Assert.That(results[1].MetricType, Is.EqualTo(2));
        Assert.That(results[2].MetricType, Is.EqualTo(3));
    }

    [Test]
    public void ToSaveGoalCommand_WithDifferentPeriodTypes_MapsDifferentPeriods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var commands = new[]
        {
            new SaveGoalSagaCommand("Goal1", "Daily goal", 1, 1, 100, startDate, null, null),
            new SaveGoalSagaCommand("Goal2", "Weekly goal", 1, 2, 100, startDate, null, null),
            new SaveGoalSagaCommand("Goal3", "Monthly goal", 1, 3, 100, startDate, null, null)
        };

        // Act
        var results = commands.Select(c => c.ToSaveGoalCommand(userId)).ToList();

        // Assert
        Assert.That(results[0].PeriodType, Is.EqualTo(1));
        Assert.That(results[1].PeriodType, Is.EqualTo(2));
        Assert.That(results[2].PeriodType, Is.EqualTo(3));
    }

    [Test]
    public void ToSaveGoalCommand_PresервesDescription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var description = "This is a detailed goal description with multiple lines\nand special characters: !@#$%";

        var command = new SaveGoalSagaCommand(
            "Complex Goal",
            description,
            1,
            1,
            50,
            startDate,
            null,
            null);

        // Act
        var result = command.ToSaveGoalCommand(userId);

        // Assert
        Assert.That(result.Description, Is.EqualTo(description));
    }
}
