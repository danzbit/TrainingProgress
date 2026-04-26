using Shared.Api.Responses;

namespace Shared.Abstractions.Idempotency;

public interface IIdempotencyService
{
    Task<IdempotencyResponse?> GetResponseAsync(
        string method, 
        string path, 
        string idempotencyKey, 
        CancellationToken ct = default);
    
    Task SaveResponseAsync(
        string method,
        string path,
        string idempotencyKey,
        IdempotencyResponse response,
        CancellationToken ct = default);
}