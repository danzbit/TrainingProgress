using Shared.Contracts.Idempotency;

namespace Shared.Abstractions.Idempotency;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetByKeyAsync(
        string method, 
        string path, 
        string idempotencyKey, 
        CancellationToken ct = default);
    
    Task SaveAsync(
        IdempotencyRecord record, 
        CancellationToken ct = default);
}