namespace AiChatService.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = string.Empty;  // "user" | "assistant"
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public ChatMessage() { }

    public static ChatMessage CreateUserMessage(Guid userId, string content) =>
        new() { Id = Guid.NewGuid(), UserId = userId, Role = "user", Content = content, CreatedAt = DateTime.UtcNow };

    public static ChatMessage CreateAssistantMessage(Guid userId, string content) =>
        new() { Id = Guid.NewGuid(), UserId = userId, Role = "assistant", Content = content, CreatedAt = DateTime.UtcNow };
}
