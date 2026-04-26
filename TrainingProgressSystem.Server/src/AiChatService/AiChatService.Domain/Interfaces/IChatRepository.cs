using AiChatService.Domain.Entities;

namespace AiChatService.Domain.Interfaces;

public interface IChatRepository
{
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);
    Task ClearHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}
