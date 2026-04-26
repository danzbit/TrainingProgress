using Microsoft.Extensions.Caching.Distributed;

namespace Shared.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    
    Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions? options = null,
        CancellationToken ct = default);

    Task RemoveAsync(string key, CancellationToken ct = default);
}