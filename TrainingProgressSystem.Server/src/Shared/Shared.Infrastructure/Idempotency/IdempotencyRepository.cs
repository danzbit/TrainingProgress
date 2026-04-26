using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Abstractions.Idempotency;
using Shared.Contracts.Idempotency;

namespace Shared.Infrastructure.Idempotency;

public class IdempotencyRepository(IDbContext dbContext) : IIdempotencyRepository
{
    public async Task<IdempotencyRecord?> GetByKeyAsync(
        string method,
        string path,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        return await dbContext.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Method == method &&
                r.Path == path &&
                r.IdempotencyKey == idempotencyKey,
                ct);
    }

    public async Task SaveAsync(IdempotencyRecord record, CancellationToken ct = default)
    {
        try
        {
            dbContext.IdempotencyRecords.Add(record);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            var alreadySaved = await dbContext.IdempotencyRecords.AsNoTracking()
                .AnyAsync(r =>
                    r.Method == record.Method &&
                    r.Path == record.Path &&
                    r.IdempotencyKey == record.IdempotencyKey,
                    ct);

            if (!alreadySaved)
            {
                throw;
            }
        }
        catch (DbUpdateException)
        {
            var alreadySaved = await dbContext.IdempotencyRecords.AsNoTracking()
                .AnyAsync(r =>
                    r.Method == record.Method &&
                    r.Path == record.Path &&
                    r.IdempotencyKey == record.IdempotencyKey,
                    ct);

            if (!alreadySaved)
            {
                throw;
            }
        }
    }
}