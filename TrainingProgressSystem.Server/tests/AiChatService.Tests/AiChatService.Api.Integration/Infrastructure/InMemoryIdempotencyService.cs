using Shared.Abstractions.Idempotency;
using Shared.Api.Responses;

namespace AiChatService.Api.Integration.Infrastructure;

internal sealed class InMemoryIdempotencyService : IIdempotencyService
{
    private readonly Dictionary<string, IdempotencyResponse> _responses = new(StringComparer.Ordinal);

    public Task<IdempotencyResponse?> GetResponseAsync(
        string method,
        string path,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        _responses.TryGetValue(CreateKey(method, path, idempotencyKey), out var response);
        return Task.FromResult(response);
    }

    public Task SaveResponseAsync(
        string method,
        string path,
        string idempotencyKey,
        IdempotencyResponse response,
        CancellationToken ct = default)
    {
        _responses[CreateKey(method, path, idempotencyKey)] = response;
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _responses.Clear();
    }

    private static string CreateKey(string method, string path, string idempotencyKey)
        => $"{method}:{path}:{idempotencyKey}";
}
