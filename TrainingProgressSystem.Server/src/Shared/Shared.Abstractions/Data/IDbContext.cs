using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Idempotency;

namespace Shared.Abstractions.Data;

public interface IDbContext
{
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}