using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using TrainingService.Domain.Entities;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class WorkoutRepositoryTests
{
    [Test]
    public async Task GetAllAsync_ReturnsWorkoutsWithRelations()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var workout = SeedWorkoutGraph(db);
        var repository = new WorkoutRepository(db);

        var result = await repository.GetAllAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value[0].Id, Is.EqualTo(workout.Id));
        Assert.That(result.Value[0].WorkoutType.Name, Is.EqualTo("Strength"));
        Assert.That(result.Value[0].Exercises.Count, Is.EqualTo(1));
        Assert.That(result.Value[0].Exercises.First().ExerciseType.Name, Is.EqualTo("Squat"));
    }

    [Test]
    public async Task GetByIdAsync_WhenWorkoutExists_ReturnsWorkout()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var workout = SeedWorkoutGraph(db);
        var repository = new WorkoutRepository(db);

        var result = await repository.GetByIdAsync(workout.Id);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Id, Is.EqualTo(workout.Id));
        Assert.That(result.Value.WorkoutType, Is.Not.Null);
        Assert.That(result.Value.Exercises.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_WhenWorkoutDoesNotExist_ReturnsNull()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new WorkoutRepository(db);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task AddAsync_WhenWorkoutIsValid_ReturnsSuccess()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var workoutTypeId = Guid.NewGuid();

        db.WorkoutTypes.Add(new WorkoutType { Id = workoutTypeId, Name = "Cardio", Description = "desc" });
        await db.SaveChangesAsync();

        var repository = new WorkoutRepository(db);
        var workout = new Workout(Guid.NewGuid(), workoutTypeId, DateTime.UtcNow.Date)
        {
            Id = Guid.NewGuid(),
            DurationMin = 35,
            CreatedAt = DateTime.UtcNow
        };

        var result = await repository.AddAsync(workout);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.Workouts.AnyAsync(w => w.Id == workout.Id), Is.True);
    }

    [Test]
    public async Task UpdateAsync_WhenWorkoutDoesNotExist_ReturnsEntityNotFound()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new WorkoutRepository(db);
        var workout = new Workout(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date)
        {
            Id = Guid.NewGuid(),
            DurationMin = 20,
            CreatedAt = DateTime.UtcNow
        };

        var result = await repository.UpdateAsync(workout);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }

    [Test]
    public async Task UpdateAsync_WhenWorkoutExists_UpdatesWorkout()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var workout = SeedWorkoutGraph(db);
        var repository = new WorkoutRepository(db);

        workout.DurationMin = 90;
        workout.Notes = "Updated";

        var result = await repository.UpdateAsync(workout);

        Assert.That(result.IsFailure, Is.False);

        var updated = await db.Workouts.FirstAsync(w => w.Id == workout.Id);
        Assert.That(updated.DurationMin, Is.EqualTo(90));
        Assert.That(updated.Notes, Is.EqualTo("Updated"));
    }

    [Test]
    public async Task DeleteAsync_WhenWorkoutDoesNotExist_ReturnsEntityNotFound()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new WorkoutRepository(db);

        var result = await repository.DeleteAsync(Guid.NewGuid());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }

    [Test]
    public async Task DeleteAsync_WhenWorkoutExists_RemovesWorkout()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var workout = SeedWorkoutGraph(db);
        var repository = new WorkoutRepository(db);

        var result = await repository.DeleteAsync(workout.Id);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.Workouts.AnyAsync(w => w.Id == workout.Id), Is.False);
    }

    private static Workout SeedWorkoutGraph(TrainingService.Infrastructure.Data.TrainingServiceDbContext db)
    {
        var workoutTypeId = Guid.NewGuid();
        var exerciseTypeId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        db.WorkoutTypes.Add(new WorkoutType
        {
            Id = workoutTypeId,
            Name = "Strength",
            Description = "desc"
        });

        db.ExerciseTypes.Add(new ExerciseType
        {
            Id = exerciseTypeId,
            Name = "Squat",
            Category = "Strength"
        });

        var workout = new Workout(Guid.NewGuid(), workoutTypeId, DateTime.UtcNow.Date)
        {
            Id = workoutId,
            DurationMin = 60,
            CreatedAt = DateTime.UtcNow,
            Notes = "Initial"
        };

        workout.Exercises.Add(new Exercise(exerciseTypeId, 3, 8)
        {
            Id = Guid.NewGuid(),
            WorkoutId = workoutId,
            WeightKg = 100
        });

        db.Workouts.Add(workout);
        db.SaveChanges();

        return workout;
    }
}