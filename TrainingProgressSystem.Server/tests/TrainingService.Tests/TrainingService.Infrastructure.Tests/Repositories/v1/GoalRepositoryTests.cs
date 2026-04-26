using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class GoalRepositoryTests
{
    [Test]
    public async Task AddAsync_CreatesGoalAndInitialProgress()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new GoalRepository(db);
        var goal = CreateGoal(GoalStatus.Active, 10);

        var result = await repository.AddAsync(goal);

        Assert.That(result.IsFailure, Is.False);

        var progress = await db.GoalProgresses.FirstOrDefaultAsync(gp => gp.GoalId == goal.Id);
        Assert.That(progress, Is.Not.Null);
        Assert.That(progress!.CurrentValue, Is.EqualTo(0));
        Assert.That(progress.IsCompleted, Is.False);
        Assert.That(progress.Percentage, Is.EqualTo(0d));
    }

    [Test]
    public async Task AddAsync_WhenGoalAlreadyCompleted_InitializesCompletedProgress()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new GoalRepository(db);
        var goal = CreateGoal(GoalStatus.Completed, 7);

        var result = await repository.AddAsync(goal);

        Assert.That(result.IsFailure, Is.False);

        var progress = await db.GoalProgresses.FirstAsync(gp => gp.GoalId == goal.Id);
        Assert.That(progress.CurrentValue, Is.EqualTo(7));
        Assert.That(progress.IsCompleted, Is.True);
        Assert.That(progress.Percentage, Is.EqualTo(100d));
    }

    [Test]
    public async Task GetByIdAsync_WhenGoalExists_ReturnsGoalWithProgress()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var goal = CreateGoal(GoalStatus.Active, 12);
        db.Goals.Add(goal);
        db.GoalProgresses.Add(new GoalProgress
        {
            GoalId = goal.Id,
            CurrentValue = 6,
            Percentage = 50,
            IsCompleted = false,
            LastCalculatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repository = new GoalRepository(db);

        var result = await repository.GetByIdAsync(goal.Id);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Progress, Is.Not.Null);
        Assert.That(result.Value.Progress!.CurrentValue, Is.EqualTo(6));
    }

    [Test]
    public async Task UpdateAsync_WhenGoalDoesNotExist_ReturnsEntityNotFound()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new GoalRepository(db);

        var result = await repository.UpdateAsync(CreateGoal(GoalStatus.Active, 5));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }

    [Test]
    public async Task UpdateAsync_WhenProgressMeetsTarget_MarksGoalAsCompleted()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var goal = CreateGoal(GoalStatus.Active, 5);
        db.Goals.Add(goal);
        db.GoalProgresses.Add(new GoalProgress
        {
            GoalId = goal.Id,
            CurrentValue = 5,
            Percentage = 50,
            IsCompleted = false,
            LastCalculatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var repository = new GoalRepository(db);

        var result = await repository.UpdateAsync(goal);

        Assert.That(result.IsFailure, Is.False);

        var updatedGoal = await db.Goals.FirstAsync(g => g.Id == goal.Id);
        var updatedProgress = await db.GoalProgresses.FirstAsync(gp => gp.GoalId == goal.Id);

        Assert.That(updatedGoal.Status, Is.EqualTo(GoalStatus.Completed));
        Assert.That(updatedProgress.IsCompleted, Is.True);
        Assert.That(updatedProgress.Percentage, Is.EqualTo(100d));
    }

    [Test]
    public async Task UpdateGoalsForWorkoutAsync_WhenWorkoutMissing_ReturnsEntityNotFound()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new GoalRepository(db);

        var result = await repository.UpdateGoalsForWorkoutAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }

    [Test]
    public async Task DeleteAsync_WhenGoalExists_RemovesGoal()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var goal = CreateGoal(GoalStatus.Active, 9);
        db.Goals.Add(goal);
        await db.SaveChangesAsync();

        var repository = new GoalRepository(db);

        var result = await repository.DeleteAsync(goal.Id);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.Goals.AnyAsync(g => g.Id == goal.Id), Is.False);
    }

    private static Goal CreateGoal(GoalStatus status, int targetValue) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Name = "Weekly Goal",
        Description = "Do workouts",
        MetricType = GoalMetricType.WorkoutCount,
        PeriodType = GoalPeriodType.Weekly,
        TargetValue = targetValue,
        Status = status,
        StartDate = DateTime.UtcNow.Date,
        EndDate = DateTime.UtcNow.Date.AddDays(7)
    };
}