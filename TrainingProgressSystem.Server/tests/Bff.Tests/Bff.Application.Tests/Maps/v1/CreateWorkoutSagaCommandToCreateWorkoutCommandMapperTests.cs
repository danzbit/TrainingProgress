using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Maps.v1;

namespace Bff.Application.Tests.Maps.v1;

[TestFixture]
public class CreateWorkoutSagaCommandToCreateWorkoutCommandMapperTests
{
    [Test]
    public void ToCreateWorkoutCommand_WithValidCommand_MapsAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = new DateTime(2026, 4, 23, 10, 30, 0, DateTimeKind.Utc);
        var workoutTypeId = Guid.NewGuid();
        var exerciseTypeId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();

        var exercises = new[]
        {
            new CreateWorkoutExerciseSagaCommand(
                exerciseId,
                exerciseTypeId,
                3,
                10,
                null,
                50m)
        };

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            45,
            "Morning strength training",
            exercises,
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Date, Is.EqualTo(date));
        Assert.That(result.WorkoutTypeId, Is.EqualTo(workoutTypeId));
        Assert.That(result.DurationMin, Is.EqualTo(45));
        Assert.That(result.Notes, Is.EqualTo("Morning strength training"));
        Assert.That(result.Exercises, Is.Not.Null);
        Assert.That(result.Exercises, Has.Count.EqualTo(1));
    }

    [Test]
    public void ToCreateWorkoutCommand_WithMultipleExercises_MapsAllExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workoutTypeId = Guid.NewGuid();

        var exercises = new[]
        {
            new CreateWorkoutExerciseSagaCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                3,
                10,
                null,
                50m),
            new CreateWorkoutExerciseSagaCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                4,
                8,
                null,
                60m),
            new CreateWorkoutExerciseSagaCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                2,
                20,
                300,
                null)
        };

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            60,
            "Full body workout",
            exercises,
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.Exercises, Has.Count.EqualTo(3));
        Assert.That(result.Exercises![0].Sets, Is.EqualTo(3));
        Assert.That(result.Exercises![1].Sets, Is.EqualTo(4));
        Assert.That(result.Exercises![2].Sets, Is.EqualTo(2));
        Assert.That(result.Exercises![0].WeightKg, Is.EqualTo(50m));
        Assert.That(result.Exercises![2].DurationSec, Is.EqualTo(300));
    }

    [Test]
    public void ToCreateWorkoutCommand_WithNullExercises_MapsNullExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workoutTypeId = Guid.NewGuid();

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            30,
            "Quick cardio",
            null,
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.Exercises, Is.Null);
    }

    [Test]
    public void ToCreateWorkoutCommand_WithEmptyExercises_MapsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workoutTypeId = Guid.NewGuid();

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            30,
            "Warm-up",
            [],
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.Exercises, Is.Empty);
    }

    [Test]
    public void ToCreateWorkoutCommand_WithNullNotes_MapsNullNotes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workoutTypeId = Guid.NewGuid();

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            30,
            null,
            null,
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.Notes, Is.Null);
    }

    [Test]
    public void ToCreateWorkoutCommand_WithNullDurationMin_MapsNullDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workoutTypeId = Guid.NewGuid();

        var command = new CreateWorkoutSagaCommand(
            date,
            workoutTypeId,
            null,
            "Open-ended workout",
            null,
            null);

        // Act
        var result = command.ToCreateWorkoutCommand(userId);

        // Assert
        Assert.That(result.DurationMin, Is.Null);
    }
}
