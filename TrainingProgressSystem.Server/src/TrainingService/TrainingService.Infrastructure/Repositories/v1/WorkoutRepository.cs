using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class WorkoutRepository(TrainingServiceDbContext dbContext) : IWorkoutRepository
{
    public async Task<ResultOfT<IReadOnlyList<Workout>>> GetAllAsync(CancellationToken ct = default)
    {
        var workouts = await dbContext.Workouts.AsNoTracking()
            .Include(w => w.WorkoutType)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.ExerciseType)
            .ToListAsync(ct);

        return ResultOfT<IReadOnlyList<Workout>>.Success(workouts);
    }

    public async Task<ResultOfT<Workout?>> GetByIdAsync(Guid workoutId, CancellationToken ct = default)
    {
        var workout = await dbContext.Workouts.AsNoTracking()
            .Include(w => w.WorkoutType)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.ExerciseType)
            .FirstOrDefaultAsync(w => w.Id == workoutId, ct);

        return ResultOfT<Workout?>.Success(workout);
    }

    public async Task<Result> AddAsync(Workout workout, CancellationToken ct = default)
    {
        await dbContext.Workouts.AddAsync(workout, ct);
        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    public async Task<Result> UpdateAsync(Workout workout, CancellationToken ct = default)
    {
        if (await WorkoutExistsAsync(workout.Id, ct) == false)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        dbContext.Workouts.Update(workout);
        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    public async Task<Result> DeleteAsync(Guid workoutId, CancellationToken ct = default)
    {
        if (await WorkoutExistsAsync(workoutId, ct) == false)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        var workout = await dbContext.Workouts.FirstOrDefaultAsync(w => w.Id == workoutId, ct);

        dbContext.Workouts.Remove(workout!);
        var result = await dbContext.SaveChangesAsync(ct);

        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }

    private async Task<bool> WorkoutExistsAsync(Guid workoutId, CancellationToken ct = default)
    {
        return await dbContext.Workouts.AnyAsync(w => w.Id == workoutId, ct);
    }
}