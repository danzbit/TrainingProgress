using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Caching;
using Shared.Caching.Models;
using Shared.Caching.Services;

namespace Shared.Caching.Tests.Services;

[TestFixture]
public class CacheServicesTests
{
    [TestCase(CacheServiceType.Distributed)]
    [TestCase(CacheServiceType.InMemory)]
    public async Task GetAsync_ReturnsDefault_WhenCacheMiss(CacheServiceType serviceType)
    {
        var cache = new FakeDistributedCache();
        var sut = CreateService(serviceType, cache);

        var result = await sut.GetAsync<SamplePayload>("missing");

        Assert.That(result, Is.Null);
    }

    [TestCase(CacheServiceType.Distributed)]
    [TestCase(CacheServiceType.InMemory)]
    public async Task SetAsync_ThenGetAsync_RoundTripsSerializedValue(CacheServiceType serviceType)
    {
        var cache = new FakeDistributedCache();
        var sut = CreateService(serviceType, cache);
        var payload = new SamplePayload { Id = 42, Name = "training" };

        await sut.SetAsync("payload", payload);
        var result = await sut.GetAsync<SamplePayload>("payload");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(42));
        Assert.That(result.Name, Is.EqualTo("training"));
    }

    [TestCase(CacheServiceType.Distributed)]
    [TestCase(CacheServiceType.InMemory)]
    public async Task SetAsync_UsesDefaultCacheOptions_WhenOptionsNotProvided(CacheServiceType serviceType)
    {
        var cache = new FakeDistributedCache();
        var sut = CreateService(serviceType, cache);

        await sut.SetAsync("k", new SamplePayload { Id = 1, Name = "n" });

        Assert.That(cache.LastSetOptions, Is.Not.Null);
        Assert.That(cache.LastSetOptions!.AbsoluteExpirationRelativeToNow, Is.EqualTo(CacheDefaults.DefaultTtl));
        Assert.That(cache.LastSetOptions.SlidingExpiration, Is.EqualTo(CacheDefaults.DefaultSlidingTtl));
    }

    [TestCase(CacheServiceType.Distributed)]
    [TestCase(CacheServiceType.InMemory)]
    public async Task SetAsync_UsesProvidedCacheOptions_WhenOptionsProvided(CacheServiceType serviceType)
    {
        var cache = new FakeDistributedCache();
        var sut = CreateService(serviceType, cache);
        var customOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
            SlidingExpiration = TimeSpan.FromSeconds(30)
        };

        await sut.SetAsync("k", new SamplePayload { Id = 2, Name = "custom" }, customOptions);

        Assert.That(cache.LastSetOptions, Is.SameAs(customOptions));
    }

    [TestCase(CacheServiceType.Distributed)]
    [TestCase(CacheServiceType.InMemory)]
    public async Task RemoveAsync_RemovesExistingValue(CacheServiceType serviceType)
    {
        var cache = new FakeDistributedCache();
        var sut = CreateService(serviceType, cache);

        await sut.SetAsync("remove-me", new SamplePayload { Id = 3, Name = "x" });
        await sut.RemoveAsync("remove-me");
        var result = await sut.GetAsync<SamplePayload>("remove-me");

        Assert.That(result, Is.Null);
    }

    private static ICacheService CreateService(CacheServiceType serviceType, IDistributedCache cache)
        => serviceType switch
        {
            CacheServiceType.Distributed => new DistributedCacheService(cache),
            CacheServiceType.InMemory => new InMemoryCacheService(cache),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unsupported cache service type")
        };

    public enum CacheServiceType
    {
        Distributed,
        InMemory
    }

    private sealed class SamplePayload
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class FakeDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _store = new(StringComparer.Ordinal);

        public DistributedCacheEntryOptions? LastSetOptions { get; private set; }

        public byte[]? Get(string key)
            => _store.TryGetValue(key, out var value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            => Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _store[key] = value;
            LastSetOptions = options;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Remove(string key)
            => _store.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}