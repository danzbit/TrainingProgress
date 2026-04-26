using AiChatService.Application.Interfaces.v1;
using AiChatService.Domain.Entities;
using Shared.Kernal.Results;

namespace AiChatService.Api.Integration.Infrastructure;

public sealed class StubChatService : IChatService
{
    public Func<string, CancellationToken, IAsyncEnumerable<string>> StreamHandler { get; set; } = default!;
    public Func<CancellationToken, Task<ResultOfT<IReadOnlyList<ChatMessage>>>> HistoryHandler { get; set; } = default!;
    public Func<CancellationToken, Task<Result>> ClearHandler { get; set; } = default!;

    public string? LastMessage { get; private set; }

    public StubChatService()
    {
        Reset();
    }

    public IAsyncEnumerable<string> SendMessageStreamAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        LastMessage = userMessage;
        return StreamHandler(userMessage, cancellationToken);
    }

    public Task<ResultOfT<IReadOnlyList<ChatMessage>>> GetHistoryAsync(CancellationToken cancellationToken = default)
        => HistoryHandler(cancellationToken);

    public Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default)
        => ClearHandler(cancellationToken);

    public void Reset()
    {
        StreamHandler = (_, _) => CreateStream("stub-chunk-1", "stub-chunk-2");
        HistoryHandler = _ => Task.FromResult(ResultOfT<IReadOnlyList<ChatMessage>>.Success(
            new List<ChatMessage>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Role = "assistant",
                    Content = "stub-history",
                    CreatedAt = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc)
                }
            }));
        ClearHandler = _ => Task.FromResult(Result.Success());
        LastMessage = null;
    }

    private static async IAsyncEnumerable<string> CreateStream(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }
}
