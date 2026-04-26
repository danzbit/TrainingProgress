using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Data;
using AnalyticsService.Infrastructure.Repositories.v1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyticsService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class WorkoutRepositoryTests
{
    [Test]
    public async Task GetAggregateByPeriodAsync_ReturnsAggregatedValues()
    {
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, typeId, "Cardio");
        SeedWorkouts(db, userId, typeId,
            (new DateTime(2026, 4, 20), 30),
            (new DateTime(2026, 4, 21), 45),
            (new DateTime(2026, 3, 1), 10));

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetAggregateByPeriodAsync(
            userId,
            new DateTime(2026, 4, 20),
            new DateTime(2026, 4, 22));

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.WorkoutsCount, Is.EqualTo(2));
        Assert.That(result.Value.DurationMin, Is.EqualTo(75));
    }

    [Test]
    public async Task GetDailyTrendByPeriodAsync_ReturnsPointsOrderedByDate()
    {
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, typeId, "Strength");
        SeedWorkouts(db, userId, typeId,
            (new DateTime(2026, 4, 20), 20),
            (new DateTime(2026, 4, 20), 30),
            (new DateTime(2026, 4, 21), 40));

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetDailyTrendByPeriodAsync(
            userId,
            new DateTime(2026, 4, 20),
            new DateTime(2026, 4, 22));

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value[0].Date, Is.EqualTo(new DateTime(2026, 4, 20)));
        Assert.That(result.Value[0].WorkoutsCount, Is.EqualTo(2));
        Assert.That(result.Value[0].DurationMin, Is.EqualTo(50));
    }

    [Test]
    public async Task GetCountByTypeAsync_ReturnsGroupedAndSortedCounts()
    {
        var userId = Guid.NewGuid();
        var cardioId = Guid.NewGuid();
        var strengthId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, cardioId, "Cardio");
        SeedWorkoutType(db, strengthId, "Strength");
        SeedWorkouts(db, userId, cardioId,
            (new DateTime(2026, 4, 20), 30),
            (new DateTime(2026, 4, 21), 25));
        SeedWorkouts(db, userId, strengthId,
            (new DateTime(2026, 4, 21), 40));

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetCountByTypeAsync(
            userId,
            new DateTime(2026, 4, 20),
            new DateTime(2026, 4, 22));

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value[0].WorkoutTypeName, Is.EqualTo("Cardio"));
        Assert.That(result.Value[0].WorkoutsCount, Is.EqualTo(2));
        Assert.That(result.Value[1].WorkoutTypeName, Is.EqualTo("Strength"));
    }

    [Test]
    public async Task GetStatisticsOverviewAsync_ReturnsExpectedOverview()
    {
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, typeId, "Cardio");
        SeedWorkouts(db, userId, typeId,
            (new DateTime(2026, 4, 20), 30),
            (new DateTime(2026, 4, 22), 40),
            (new DateTime(2026, 3, 10), 15));

        db.Goals.AddRange(
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 1 },
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 1 },
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 0 });
        await db.SaveChangesAsync();

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetStatisticsOverviewAsync(
            userId,
            new DateTime(2026, 4, 20),
            new DateTime(2026, 4, 27));

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.TotalAchievedGoals, Is.EqualTo(2));
        Assert.That(result.Value.TotalWorkoutsCompleted, Is.EqualTo(3));
        Assert.That(result.Value.TotalTrainingMinutes, Is.EqualTo(85));
        Assert.That(result.Value.WorkoutsThisWeek, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTotalWorkoutsCountAsync_ReturnsTotalCount()
    {
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, typeId, "Cardio");
        SeedWorkouts(db, userId, typeId,
            (new DateTime(2026, 4, 20), 10),
            (new DateTime(2026, 4, 21), 20));

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetTotalWorkoutsCountAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTotalDurationMinutesAsync_ReturnsTotalDuration()
    {
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        await using var db = CreateDbContext();
        SeedWorkoutType(db, typeId, "Cardio");
        SeedWorkouts(db, userId, typeId,
            (new DateTime(2026, 4, 20), 10),
            (new DateTime(2026, 4, 21), 20));

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetTotalDurationMinutesAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(30));
    }

    [Test]
    public async Task GetAchievedGoalsCountAsync_ReturnsCompletedGoalsOnly()
    {
        var userId = Guid.NewGuid();

        await using var db = CreateDbContext();
        db.Goals.AddRange(
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 1 },
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 0 },
            new Goal { Id = Guid.NewGuid(), UserId = userId, Status = 1 });
        await db.SaveChangesAsync();

        var repository = new WorkoutRepository(db, Mock.Of<ILogger<WorkoutRepository>>());

        var result = await repository.GetAchievedGoalsCountAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(2));
    }

    private static AnalyticsServiceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AnalyticsServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AnalyticsServiceDbContext(options);
    }

    private static void SeedWorkoutType(AnalyticsServiceDbContext db, Guid id, string name)
    {
        db.WorkoutTypes.Add(new WorkoutType
        {
            Id = id,
            Name = name,
            Description = name
        });
        db.SaveChanges();
    }

    private static void SeedWorkouts(AnalyticsServiceDbContext db, Guid userId, Guid workoutTypeId,
        params (DateTime Date, int DurationMin)[] workouts)
    {
        var type = db.WorkoutTypes.First(wt => wt.Id == workoutTypeId);

        foreach (var workout in workouts)
        {
            db.Workouts.Add(new Workout
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkoutTypeId = workoutTypeId,
                WorkoutType = type,
                Date = workout.Date,
                DurationMin = workout.DurationMin,
                CreatedAt = workout.Date
            });
        }

        db.SaveChanges();
    }
}
