using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class ExerciseTypeRepository(TrainingServiceDbContext dbContext) : IExerciseTypeRepository
{
    public async Task<ResultOfT<IReadOnlyList<ExerciseType>>> GetAllAsync(CancellationToken ct = default)
    {
        var types = await dbContext.ExerciseTypes.AsNoTracking().ToListAsync(ct);
        return ResultOfT<IReadOnlyList<ExerciseType>>.Success(types);
    }
}
