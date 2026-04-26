using System.Runtime.CompilerServices;
using System.Text;
using AiChatService.Application.Interfaces.v1;
using AiChatService.Domain.Entities;
using AiChatService.Domain.Interfaces;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AiChatService.Application.Services.v1;

public sealed class ChatService(
    ILlmClient llmClient,
    IChatRepository chatRepository,
    ICurrentUser currentUser) : IChatService
{
    private const string SystemPrompt = """
        You are a professional fitness trainer.

        Rules:
        - Be conversational and friendly
        - Do NOT require user info upfront
        - Give useful answers even with no data
        - Ask follow-up questions ONLY if necessary
        - Keep answers practical and structured
        - Avoid medical advice

        When missing important info:
        - Ask 1–2 short questions max
        - Do not overwhelm the user
        """;

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            yield return "[Unauthorized]";
            yield break;
        }

        var userId = userIdResult.Value;
        var history = await chatRepository.GetHistoryAsync(userId, limit: 10, cancellationToken);

        var messages = new List<(string Role, string Content)>
        {
            ("system", SystemPrompt)
        };

        foreach (var msg in history)
            messages.Add((msg.Role, msg.Content));

        messages.Add(("user", userMessage));

        var userEntry = ChatMessage.CreateUserMessage(userId, userMessage);
        await chatRepository.AddAsync(userEntry, cancellationToken);

        var assistantBuilder = new StringBuilder();

        await foreach (var chunk in llmClient.StreamCompletionAsync(messages, cancellationToken))
        {
            assistantBuilder.Append(chunk);
            yield return chunk;
        }

        var assistantEntry = ChatMessage.CreateAssistantMessage(userId, assistantBuilder.ToString());
        await chatRepository.AddAsync(assistantEntry, cancellationToken);
    }

    public async Task<ResultOfT<IReadOnlyList<ChatMessage>>> GetHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return ResultOfT<IReadOnlyList<ChatMessage>>.Failure(userIdResult.Error);

        var history = await chatRepository.GetHistoryAsync(userIdResult.Value, limit: 50, cancellationToken);
        return ResultOfT<IReadOnlyList<ChatMessage>>.Success(history);
    }

    public async Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        await chatRepository.ClearHistoryAsync(userIdResult.Value, cancellationToken);
        return Result.Success();
    }
}
