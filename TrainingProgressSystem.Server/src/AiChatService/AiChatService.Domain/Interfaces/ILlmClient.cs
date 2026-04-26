namespace AiChatService.Domain.Interfaces;

public interface ILlmClient
{
    IAsyncEnumerable<string> StreamCompletionAsync(
        IReadOnlyList<(string Role, string Content)> messages,
        CancellationToken cancellationToken = default);
}
