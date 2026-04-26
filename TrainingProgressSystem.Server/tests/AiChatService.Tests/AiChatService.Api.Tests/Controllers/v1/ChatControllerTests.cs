using AiChatService.Api.Controllers.v1;
using AiChatService.Application.Dtos.v1.Requests;
using AiChatService.Application.Interfaces.v1;
using AiChatService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using System.Text;
using System.Text.Json;

namespace AiChatService.Api.Tests.Controllers.v1;

[TestFixture]
public class ChatControllerTests
{
    private Mock<IChatService> _chatServiceMock = null!;
    private ChatController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _chatServiceMock = new Mock<IChatService>(MockBehavior.Strict);
        _controller = new ChatController(_chatServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public async Task StreamMessage_WritesSseChunksAndDoneMarker()
    {
        _chatServiceMock
            .Setup(s => s.SendMessageStreamAsync("hello", It.IsAny<CancellationToken>()))
            .Returns(CreateStream("chunk-1", "chunk-2"));

        await _controller.StreamMessage(new SendMessageRequest("hello"), CancellationToken.None);

        var response = _controller.Response;
        Assert.That(response.Headers.ContentType.ToString(), Is.EqualTo("text/event-stream"));
        Assert.That(response.Headers.CacheControl.ToString(), Is.EqualTo("no-cache"));
        Assert.That(response.Headers.Connection.ToString(), Is.EqualTo("keep-alive"));

        response.Body.Position = 0;
        var body = await new StreamReader(response.Body, Encoding.UTF8).ReadToEndAsync();

        Assert.That(body, Does.Contain($"data: {JsonSerializer.Serialize("chunk-1")}\n\n"));
        Assert.That(body, Does.Contain($"data: {JsonSerializer.Serialize("chunk-2")}\n\n"));
        Assert.That(body, Does.Contain("data: [DONE]\n\n"));

        _chatServiceMock.Verify(s => s.SendMessageStreamAsync("hello", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetHistory_WhenServiceSucceeds_ReturnsOkWithProjectedFields()
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = "assistant",
            Content = "Hello from history",
            CreatedAt = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc)
        };

        _chatServiceMock
            .Setup(s => s.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<ChatMessage>>.Success([message]));

        var result = await _controller.GetHistory(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var payloadJson = JsonSerializer.Serialize(ok!.Value);
        Assert.That(payloadJson, Does.Contain(message.Id.ToString()));
        Assert.That(payloadJson, Does.Contain("assistant"));
        Assert.That(payloadJson, Does.Contain("Hello from history"));
        Assert.That(payloadJson, Does.Not.Contain("UserId"));

        _chatServiceMock.Verify(s => s.GetHistoryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetHistory_WhenServiceFails_ReturnsNotFound()
    {
        _chatServiceMock
            .Setup(s => s.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<ChatMessage>>.Failure(
                new Error(ErrorCode.EntityNotFound, "History not found")));

        var result = await _controller.GetHistory(CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task ClearHistory_WhenServiceSucceeds_ReturnsOk()
    {
        _chatServiceMock
            .Setup(s => s.ClearHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.ClearHistory(CancellationToken.None);

        Assert.That(result, Is.TypeOf<OkResult>());
        _chatServiceMock.Verify(s => s.ClearHistoryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ClearHistory_WhenServiceFailsWithDownstreamUnavailable_Returns503()
    {
        _chatServiceMock
            .Setup(s => s.ClearHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(
                new Error(ErrorCode.DownstreamServiceUnavailable, "Chat store unavailable")));

        var result = await _controller.ClearHistory(CancellationToken.None);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
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
