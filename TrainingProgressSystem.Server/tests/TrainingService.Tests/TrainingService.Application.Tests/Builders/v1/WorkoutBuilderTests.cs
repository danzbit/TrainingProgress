using AutoMapper;
using Moq;
using TrainingService.Application.Builders.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Tests.Builders.v1;

[TestFixture]
public class WorkoutBuilderTests
{
    [Test]
    public void Build_WithNewWorkout_AppliesDurationNotesAndExercises()
    {
        var userId = Guid.NewGuid();
        var workoutTypeId = Guid.NewGuid();
        var date = new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc);

        var builder = new WorkoutBuilder(userId, workoutTypeId, date)
            .WithDuration(45)
            .WithNotes("  Leg day  ")
            .ApplyExercises([
                new ExerciseRequest(null, Guid.NewGuid(), 3, 10, 60, 40m)
            ]);

        var workout = builder.Build();

        Assert.That(workout.UserId, Is.EqualTo(userId));
        Assert.That(workout.WorkoutTypeId, Is.EqualTo(workoutTypeId));
        Assert.That(workout.Date, Is.EqualTo(date));
        Assert.That(workout.DurationMin, Is.EqualTo(45));
        Assert.That(workout.Notes, Is.EqualTo("  Leg day  "));
        Assert.That(workout.Exercises, Has.Count.EqualTo(1));
        Assert.That(workout.Exercises.First().Sets, Is.EqualTo(3));
    }

    [Test]
    public void ApplyExercises_WithExistingWorkout_UpdatesOnlyMatchingExercises()
    {
        var exerciseId = Guid.NewGuid();

        var mappedWorkout = new Workout(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow)
        {
            Exercises =
            [
                new Exercise(Guid.NewGuid(), 1, 1, 10m, 10)
                {
                    Id = exerciseId
                }
            ]
        };

        var existingRequest = new UpdateWorkoutRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            20,
            "notes",
            []);

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(x => x.Map<Workout>(existingRequest))
            .Returns(mappedWorkout);

        var builder = new WorkoutBuilder(existingRequest, mapperMock.Object)
            .ApplyExercises(
            [
                new ExerciseRequest(exerciseId, Guid.NewGuid(), 4, 12, null, null),
                new ExerciseRequest(Guid.NewGuid(), Guid.NewGuid(), 9, 9, null, 99m)
            ]);

        var workout = builder.Build();
        var updated = workout.Exercises.Single();

        Assert.That(updated.Sets, Is.EqualTo(4));
        Assert.That(updated.Reps, Is.EqualTo(12));
        Assert.That(updated.WeightKg, Is.EqualTo(0m));
        Assert.That(workout.Exercises, Has.Count.EqualTo(1));
    }
}
