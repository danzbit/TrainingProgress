using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Caching;
using Shared.Abstractions.Idempotency;
using Shared.Api.Responses;
using Shared.Caching.Models;
using Shared.Contracts.Idempotency;

namespace Shared.Api.Idempotency;

public class IdempotencyService(
    ICacheService cache,
    IIdempotencyRepository? repository = null) : IIdempotencyService
{
    private const int IdempotencyCacheDurationMinutes = 24;

    public async Task<IdempotencyResponse?> GetResponseAsync(
        string method,
        string path,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        var cacheKey = IdempotencyCacheKeys.Response(method, path, idempotencyKey);

        var cachedResponse = await cache.GetAsync<IdempotencyResponse>(cacheKey, ct);
        if (cachedResponse != null)
            return cachedResponse;

        if (repository is null)
        {
            return null;
        }

        var record = await repository.GetByKeyAsync(method, path, idempotencyKey, ct);
        if (record == null) return null;
        var response = new IdempotencyResponse
        {
            StatusCode = record.StatusCode,
            Body = record.ResponseBody,
            Headers = record.Headers
        };

        await cache.SetAsync(
            cacheKey,
            response,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(IdempotencyCacheDurationMinutes)
            },
            ct);

        return response;

    }

    public async Task SaveResponseAsync(
        string method,
        string path,
        string idempotencyKey,
        IdempotencyResponse response,
        CancellationToken ct = default)
    {
        var cacheKey = IdempotencyCacheKeys.Response(method, path, idempotencyKey);

        await cache.SetAsync(
            cacheKey,
            response,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(IdempotencyCacheDurationMinutes)
            },
            ct);

        if (repository is null)
        {
            return;
        }

        var existingRecord = await repository.GetByKeyAsync(method, path, idempotencyKey, ct);
        if (existingRecord != null)
            return;

        var record = new IdempotencyRecord
        (
            idempotencyKey,
            method,
            path,
            response.StatusCode,
            response.Body,
            response.Headers,
            DateTime.UtcNow
        );

        await repository.SaveAsync(record, ct);
    }
}