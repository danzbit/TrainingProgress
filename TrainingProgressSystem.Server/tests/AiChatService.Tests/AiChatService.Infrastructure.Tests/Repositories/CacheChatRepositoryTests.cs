using AiChatService.Domain.Entities;
using AiChatService.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Caching;

namespace AiChatService.Infrastructure.Tests.Repositories;

[TestFixture]
public class CacheChatRepositoryTests
{
    [Test]
    public async Task AddAsync_AppendsMessageToHistory()
    {
        var cache = new InMemoryCacheService();
        var repository = new CacheChatRepository(cache);
        var userId = Guid.NewGuid();

        await repository.AddAsync(new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = "user",
            Content = "hello",
            CreatedAt = DateTime.UtcNow
        });

        var history = await repository.GetHistoryAsync(userId, 20);

        Assert.That(history, Has.Count.EqualTo(1));
        Assert.That(history[0].Content, Is.EqualTo("hello"));
        Assert.That(cache.LastSetOptions, Is.Not.Null);
        Assert.That(cache.LastSetOptions!.SlidingExpiration, Is.EqualTo(TimeSpan.FromHours(24)));
    }

    [Test]
    public async Task AddAsync_WhenHistoryExceedsMaxStoredMessages_TrimsOldestMessages()
    {
        var cache = new InMemoryCacheService();
        var repository = new CacheChatRepository(cache);
        var userId = Guid.NewGuid();

        for (var i = 0; i < 105; i++)
        {
            await repository.AddAsync(new ChatMessage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = "assistant",
                Content = $"msg-{i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }

        var history = await repository.GetHistoryAsync(userId, 200);

        Assert.That(history, Has.Count.EqualTo(100));
        Assert.That(history[0].Content, Is.EqualTo("msg-5"));
        Assert.That(history[^1].Content, Is.EqualTo("msg-104"));
    }

    [Test]
    public async Task GetHistoryAsync_WhenLimitIsLower_ReturnsMostRecentMessagesOnly()
    {
        var cache = new InMemoryCacheService();
        var repository = new CacheChatRepository(cache);
        var userId = Guid.NewGuid();

        await repository.AddAsync(CreateMessage(userId, "one"));
        await repository.AddAsync(CreateMessage(userId, "two"));
        await repository.AddAsync(CreateMessage(userId, "three"));

        var history = await repository.GetHistoryAsync(userId, 2);

        Assert.That(history.Select(m => m.Content), Is.EqualTo(new[] { "two", "three" }));
    }

    [Test]
    public async Task ClearHistoryAsync_RemovesStoredHistory()
    {
        var cache = new InMemoryCacheService();
        var repository = new CacheChatRepository(cache);
        var userId = Guid.NewGuid();

        await repository.AddAsync(CreateMessage(userId, "to-clear"));
        await repository.ClearHistoryAsync(userId);

        var history = await repository.GetHistoryAsync(userId, 20);

        Assert.That(history, Is.Empty);
    }

    private static ChatMessage CreateMessage(Guid userId, string content) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Role = "assistant",
        Content = content,
        CreatedAt = DateTime.UtcNow
    };

    private sealed class InMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

        public DistributedCacheEntryOptions? LastSetOptions { get; private set; }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            return Task.FromResult(_values.TryGetValue(key, out var value) ? (T?)value : default);
        }

        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken ct = default)
        {
            _values[key] = value;
            LastSetOptions = options;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}
