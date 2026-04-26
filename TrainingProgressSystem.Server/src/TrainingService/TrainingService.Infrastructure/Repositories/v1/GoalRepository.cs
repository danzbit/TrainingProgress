using System.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class GoalRepository(TrainingServiceDbContext dbContext) : IGoalRepository
{
    public async Task<ResultOfT<IReadOnlyList<Goal>>> GetAllAsync(CancellationToken ct = default)
    {
        var goals = await dbContext.Goals.AsNoTracking()
            .Include(g => g.Progress)
            .ToListAsync(ct);

        return ResultOfT<IReadOnlyList<Goal>>.Success(goals);
    }

    public async Task<ResultOfT<Goal?>> GetByIdAsync(Guid goalId, CancellationToken ct = default)
    {
        var goal = await dbContext.Goals.AsNoTracking()
            .Include(g => g.Progress)
            .FirstOrDefaultAsync(g => g.Id == goalId, ct);

        return ResultOfT<Goal?>.Success(goal);
    }

    public async Task<Result> AddAsync(Goal goal, CancellationToken ct = default)
    {
        await dbContext.Goals.AddAsync(goal, ct);

        var initialCurrentValue = goal.Status == GoalStatus.Completed ? goal.TargetValue : 0;
        await dbContext.GoalProgresses.AddAsync(new GoalProgress
        {
            GoalId = goal.Id,
            CurrentValue = initialCurrentValue,
            Percentage = CalculatePercentage(initialCurrentValue, goal.TargetValue),
            IsCompleted = goal.Status == GoalStatus.Completed,
            LastCalculatedAt = DateTime.UtcNow
        }, ct);

        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    public async Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default)
    {
        var workoutExists = await dbContext.Workouts.AsNoTracking()
            .AnyAsync(w => w.Id == workoutId && w.UserId == userId, ct);

        if (!workoutExists)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.sp_UpdateGoalsForWorkout @UserId={userId}, @WorkoutId={workoutId}",
            ct);

        return Result.Success();
    }

    public async Task<ResultOfT<int>> RecalculateGoalsProgressAsync(Guid userId, Guid? goalId = null, CancellationToken ct = default)
    {
        var recalculatedCountParameter = new Microsoft.Data.SqlClient.SqlParameter(
            "@RecalculatedCount",
            SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var goalIdParameter = new Microsoft.Data.SqlClient.SqlParameter(
            "@GoalId",
            SqlDbType.UniqueIdentifier)
        {
            Value = goalId.HasValue ? goalId.Value : DBNull.Value
        };

        await dbContext.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_RecalculateGoalsProgress @UserId=@UserId, @RecalculatedCount=@RecalculatedCount OUTPUT, @GoalId=@GoalId",
            new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
            recalculatedCountParameter,
            goalIdParameter);

        var recalculatedCount = (int?)recalculatedCountParameter.Value ?? 0;
        return ResultOfT<int>.Success(recalculatedCount);
    }

    public async Task<Result> UpdateAsync(Goal goal, CancellationToken ct = default)
    {
        var exists = await dbContext.Goals.AsNoTracking().AnyAsync(g => g.Id == goal.Id, ct);
        if (!exists)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        var progress = await dbContext.GoalProgresses.FirstOrDefaultAsync(gp => gp.GoalId == goal.Id, ct);
        if (progress is null)
        {
            var initialCurrentValue = goal.Status == GoalStatus.Completed ? goal.TargetValue : 0;
            await dbContext.GoalProgresses.AddAsync(new GoalProgress
            {
                GoalId = goal.Id,
                CurrentValue = initialCurrentValue,
                Percentage = CalculatePercentage(initialCurrentValue, goal.TargetValue),
                IsCompleted = goal.Status == GoalStatus.Completed,
                LastCalculatedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            if (goal.Status == GoalStatus.Active && progress.CurrentValue >= goal.TargetValue)
            {
                goal.Status = GoalStatus.Completed;
            }

            progress.Percentage = CalculatePercentage(progress.CurrentValue, goal.TargetValue);
            progress.IsCompleted = goal.Status == GoalStatus.Completed || progress.CurrentValue >= goal.TargetValue;
            progress.LastCalculatedAt = DateTime.UtcNow;
        }

        dbContext.Goals.Update(goal);
        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    public async Task<Result> DeleteAsync(Guid goalId, CancellationToken ct = default)
    {
        var goal = await dbContext.Goals.FirstOrDefaultAsync(g => g.Id == goalId, ct);
        if (goal is null)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        dbContext.Goals.Remove(goal);
        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    private static double CalculatePercentage(int currentValue, int targetValue)
    {
        if (targetValue <= 0)
        {
            return 0d;
        }

        var raw = currentValue * 100d / targetValue;
        return Math.Clamp(raw, 0d, 100d);
    }
}
