using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AiChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiChatService.Infrastructure.Llm;

/// <summary>
/// Groq API client that streams Llama-3 completions via server-sent events.
/// Uses IHttpClientFactory for connection pooling and applies resilience policies
/// (retry with exponential back-off + circuit breaker) configured at registration time.
/// </summary>
internal sealed class GroqLlmClient(
    IHttpClientFactory httpClientFactory,
    ILogger<GroqLlmClient> logger,
    IOptions<GroqOptions> options) : ILlmClient
{
    internal const string ClientName = "groq";

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        IReadOnlyList<(string Role, string Content)> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = options.Value.Model,
            stream = true,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/openai/v1/chat/completions")
        {
            Content = JsonContent.Create(payload)
        };

        // Create a named client — resilience pipeline (retry + circuit breaker) is wired in DI
        using var httpClient = httpClientFactory.CreateClient(ClientName);

        HttpResponseMessage response;
        bool failed = false;
        string failureMessage = string.Empty;

        try
        {
            response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Groq API returned {StatusCode}. Body: {Body}",
                    (int)response.StatusCode, body);
                failed = true;
                failureMessage = $"[AI service error: {(int)response.StatusCode}]";
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request to Groq API failed (all retries exhausted)");
            failed = true;
            failureMessage = "[AI service is temporarily unavailable. Please try again in a moment.]";
            response = null!;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Groq API request timed out");
            failed = true;
            failureMessage = "[AI service timed out. Please try again.]";
            response = null!;
        }

        if (failed)
        {
            yield return failureMessage;
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;

            var data = line["data: ".Length..].Trim();
            if (data == "[DONE]") break;

            string? chunk = null;
            try
            {
                using var doc = JsonDocument.Parse(data);

                // The "content" key may be absent on the final delta — GetString returns null safely
                var deltaElement = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta");

                if (deltaElement.TryGetProperty("content", out var contentProp))
                    chunk = contentProp.GetString();
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to parse SSE chunk: {Data}", data);
            }

            if (!string.IsNullOrEmpty(chunk))
                yield return chunk;
        }
    }
}
