using AiChatService.Application.Dtos.v1.Requests;
using AiChatService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;

namespace AiChatService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    /// <summary>
    /// Send a message and receive an SSE-streamed response from the LLM.
    /// </summary>
    [HttpPost("stream")]
    public async Task StreamMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await Response.Body.FlushAsync(cancellationToken);

        await foreach (var chunk in chatService.SendMessageStreamAsync(request.Message, cancellationToken))
        {
            var data = $"data: {System.Text.Json.JsonSerializer.Serialize(chunk)}\n\n";
            await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(data), cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.Body.WriteAsync("data: [DONE]\n\n"u8.ToArray(), cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Get chat message history for the current user.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var result = await chatService.GetHistoryAsync(cancellationToken);
        return result.ToActionResult(result.Value?.Select(m => new
        {
            m.Id,
            m.Role,
            m.Content,
            m.CreatedAt
        }));
    }

    /// <summary>
    /// Delete all chat history for the current user.
    /// </summary>
    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory(CancellationToken cancellationToken)
    {
        var result = await chatService.ClearHistoryAsync(cancellationToken);
        return result.ToActionResult();
    }
}

