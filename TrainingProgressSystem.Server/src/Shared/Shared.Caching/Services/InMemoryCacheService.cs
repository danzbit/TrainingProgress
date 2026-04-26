using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Caching;
using Shared.Caching.Models;

namespace Shared.Caching.Services;

public class InMemoryCacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var data = await cache.GetAsync(key, ct);
        return data is null
            ? default
            : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDefaults.DefaultTtl,
                SlidingExpiration = CacheDefaults.DefaultSlidingTtl
            };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await cache.SetAsync(key, bytes, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => cache.RemoveAsync(key, ct);
}