using AiChatService.Domain.Entities;
using AiChatService.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Caching;

namespace AiChatService.Infrastructure.Repositories;

public sealed class CacheChatRepository(ICacheService cache) : IChatRepository
{
    private const int MaxStoredMessages = 100;
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromHours(24)
    };

    private static string KeyFor(Guid userId) => $"chat:history:{userId}";

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        var key = KeyFor(message.UserId);
        var history = await cache.GetAsync<List<ChatMessage>>(key, cancellationToken) ?? [];
        history.Add(message);

        if (history.Count > MaxStoredMessages)
            history = history[^MaxStoredMessages..];

        await cache.SetAsync(key, history, CacheOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(
        Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        var history = await cache.GetAsync<List<ChatMessage>>(KeyFor(userId), cancellationToken) ?? [];
        return history.Count <= limit
            ? history
            : history[^limit..];
    }

    public Task ClearHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(KeyFor(userId), cancellationToken);
}
