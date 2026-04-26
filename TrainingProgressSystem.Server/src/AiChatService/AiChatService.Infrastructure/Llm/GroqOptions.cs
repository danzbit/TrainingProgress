namespace AiChatService.Infrastructure.Llm;

internal sealed class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}
