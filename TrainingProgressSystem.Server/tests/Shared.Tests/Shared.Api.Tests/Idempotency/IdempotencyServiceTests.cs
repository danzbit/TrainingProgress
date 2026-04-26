using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Shared.Abstractions.Caching;
using Shared.Abstractions.Idempotency;
using Shared.Api.Idempotency;
using Shared.Api.Responses;
using Shared.Caching.Models;
using Shared.Contracts.Idempotency;

namespace Shared.Api.Tests.Idempotency;

[TestFixture]
public class IdempotencyServiceTests
{
    [Test]
    public async Task GetResponseAsync_ReturnsCachedResponse_WhenPresentInCache()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var repository = new Mock<IIdempotencyRepository>(MockBehavior.Strict);
        var expected = new IdempotencyResponse { StatusCode = 200, Body = "cached" };
        var cacheKey = IdempotencyCacheKeys.Response("POST", "/api/workouts", "idem-1");

        cache.Setup(c => c.GetAsync<IdempotencyResponse>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new IdempotencyService(cache.Object, repository.Object);

        var result = await sut.GetResponseAsync("POST", "/api/workouts", "idem-1");

        Assert.That(result, Is.SameAs(expected));
        repository.Verify(r => r.GetByKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetResponseAsync_ReturnsNull_WhenCacheMissAndRepositoryIsMissing()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var cacheKey = IdempotencyCacheKeys.Response("POST", "/api/workouts", "idem-2");

        cache.Setup(c => c.GetAsync<IdempotencyResponse>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyResponse?)null);

        var sut = new IdempotencyService(cache.Object);

        var result = await sut.GetResponseAsync("POST", "/api/workouts", "idem-2");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetResponseAsync_ReturnsRepositoryResponse_AndStoresItInCache_WhenCacheMisses()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var repository = new Mock<IIdempotencyRepository>(MockBehavior.Strict);
        var cacheKey = IdempotencyCacheKeys.Response("PATCH", "/api/goals/1", "idem-3");
        var record = new IdempotencyRecord(
            "idem-3",
            "PATCH",
            "/api/goals/1",
            202,
            "{\"ok\":true}",
            new Dictionary<string, string> { ["X-Test"] = "1" },
            DateTime.UtcNow);

        cache.Setup(c => c.GetAsync<IdempotencyResponse>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyResponse?)null);
        repository.Setup(r => r.GetByKeyAsync("PATCH", "/api/goals/1", "idem-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        cache.Setup(c => c.SetAsync(
                cacheKey,
                It.Is<IdempotencyResponse>(response =>
                    response.StatusCode == 202 &&
                    response.Body == "{\"ok\":true}" &&
                    response.Headers["X-Test"] == "1"),
                It.Is<DistributedCacheEntryOptions>(options =>
                    options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(24)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new IdempotencyService(cache.Object, repository.Object);

        var result = await sut.GetResponseAsync("PATCH", "/api/goals/1", "idem-3");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(202));
        Assert.That(result.Body, Is.EqualTo("{\"ok\":true}"));
        Assert.That(result.Headers["X-Test"], Is.EqualTo("1"));
        cache.VerifyAll();
        repository.VerifyAll();
    }

    [Test]
    public async Task SaveResponseAsync_SavesToCacheAndRepository_WhenNoExistingRecord()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var repository = new Mock<IIdempotencyRepository>(MockBehavior.Strict);
        var response = new IdempotencyResponse
        {
            StatusCode = 201,
            Body = "created",
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
        var cacheKey = IdempotencyCacheKeys.Response("POST", "/api/workouts", "idem-4");

        cache.Setup(c => c.SetAsync(
                cacheKey,
                response,
                It.Is<DistributedCacheEntryOptions>(options =>
                    options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(24)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.GetByKeyAsync("POST", "/api/workouts", "idem-4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);
        repository.Setup(r => r.SaveAsync(
                It.Is<IdempotencyRecord>(record =>
                    record.IdempotencyKey == "idem-4" &&
                    record.Method == "POST" &&
                    record.Path == "/api/workouts" &&
                    record.StatusCode == 201 &&
                    record.ResponseBody == "created" &&
                    record.Headers["Content-Type"] == "application/json"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new IdempotencyService(cache.Object, repository.Object);

        await sut.SaveResponseAsync("POST", "/api/workouts", "idem-4", response);

        cache.VerifyAll();
        repository.VerifyAll();
    }

    [Test]
    public async Task SaveResponseAsync_DoesNotPersistRecord_WhenRepositoryAlreadyHasRecord()
    {
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var repository = new Mock<IIdempotencyRepository>(MockBehavior.Strict);
        var response = new IdempotencyResponse { StatusCode = 200, Body = "ok" };
        var cacheKey = IdempotencyCacheKeys.Response("PUT", "/api/profile", "idem-5");

        cache.Setup(c => c.SetAsync(
                cacheKey,
                response,
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.GetByKeyAsync("PUT", "/api/profile", "idem-5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdempotencyRecord("idem-5", "PUT", "/api/profile", 200, "ok", new Dictionary<string, string>(), DateTime.UtcNow));

        var sut = new IdempotencyService(cache.Object, repository.Object);

        await sut.SaveResponseAsync("PUT", "/api/profile", "idem-5", response);

        repository.Verify(r => r.SaveAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}