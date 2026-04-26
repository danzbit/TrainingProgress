using AiChatService.Domain.Entities;
using Shared.Kernal.Results;

namespace AiChatService.Application.Interfaces.v1;

public interface IChatService
{
    IAsyncEnumerable<string> SendMessageStreamAsync(
        string userMessage,
        CancellationToken cancellationToken = default);

    Task<ResultOfT<IReadOnlyList<ChatMessage>>> GetHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default);
}
