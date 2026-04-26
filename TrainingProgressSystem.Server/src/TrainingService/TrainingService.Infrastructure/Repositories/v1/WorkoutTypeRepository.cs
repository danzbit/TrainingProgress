using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class WorkoutTypeRepository(TrainingServiceDbContext dbContext) : IWorkoutTypeRepository
{
    public async Task<ResultOfT<IReadOnlyList<WorkoutType>>> GetAllAsync(CancellationToken ct = default)
    {
        var types = await dbContext.WorkoutTypes.AsNoTracking().ToListAsync(ct);
        return ResultOfT<IReadOnlyList<WorkoutType>>.Success(types);
    }
}
