using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class UserPreferenceRepository(TrainingServiceDbContext dbContext) : IUserPreferenceRepository
{
    public async Task<ResultOfT<UserPreference?>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var preference = await dbContext.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        return ResultOfT<UserPreference?>.Success(preference);
    }

    public async Task<Result> UpsertAsync(UserPreference preference, CancellationToken ct = default)
    {
        var existing = await dbContext.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == preference.UserId, ct);

        if (existing is null)
            dbContext.UserPreferences.Add(preference);
        else
            existing.HistoryViewMode = preference.HistoryViewMode;

        var result = await dbContext.SaveChangesAsync(ct);
        return result > 0 ? Result.Success() : Result.Failure(Error.UnexpectedError);
    }
}
