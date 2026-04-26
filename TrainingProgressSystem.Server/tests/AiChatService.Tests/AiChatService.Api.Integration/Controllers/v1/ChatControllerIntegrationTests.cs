using System.Net;
using System.Net.Http.Json;
using System.Text;
using AiChatService.Api.Integration.Infrastructure;
using AiChatService.Application.Dtos.v1.Requests;
using AiChatService.Domain.Entities;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AiChatService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class ChatControllerIntegrationTests
{
    private AiChatApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new AiChatApiFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.ChatService.Reset();
        _factory.IdempotencyService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task StreamMessage_WhenAuthenticated_ReturnsSsePayload()
    {
        _factory.ChatService.StreamHandler = (message, _) => CreateStream($"echo:{message}", "done-chunk");

        var response = await _client.PostAsJsonAsync("/api/v1/chat/stream", new SendMessageRequest("hello"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/event-stream"));
        Assert.That(_factory.ChatService.LastMessage, Is.EqualTo("hello"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("data: \"echo:hello\"\n\n"));
        Assert.That(body, Does.Contain("data: \"done-chunk\"\n\n"));
        Assert.That(body, Does.Contain("data: [DONE]\n\n"));
    }

    [Test]
    public async Task GetHistory_WhenServiceReturnsSuccess_ReturnsProjectedPayload()
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = "assistant",
            Content = "History message",
            CreatedAt = new DateTime(2026, 4, 21, 12, 0, 0, DateTimeKind.Utc)
        };

        _factory.ChatService.HistoryHandler = _ => Task.FromResult(ResultOfT<IReadOnlyList<ChatMessage>>.Success([message]));

        var response = await _client.GetAsync("/api/v1/chat/history");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain(message.Id.ToString()));
        Assert.That(payload, Does.Contain("assistant"));
        Assert.That(payload, Does.Contain("History message"));
        Assert.That(payload, Does.Not.Contain("UserId"));
    }

    [Test]
    public async Task GetHistory_WhenServiceReturnsNotFound_Returns404()
    {
        _factory.ChatService.HistoryHandler = _ => Task.FromResult(ResultOfT<IReadOnlyList<ChatMessage>>.Failure(
            new Error(ErrorCode.EntityNotFound, "History not found")));

        var response = await _client.GetAsync("/api/v1/chat/history");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("History not found"));
    }

    [Test]
    public async Task ClearHistory_WhenServiceReturnsSuccess_Returns200()
    {
        var response = await _client.DeleteAsync("/api/v1/chat/history");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ClearHistory_WhenServiceReturnsDownstreamUnavailable_Returns503()
    {
        _factory.ChatService.ClearHandler = _ => Task.FromResult(Result.Failure(
            new Error(ErrorCode.DownstreamServiceUnavailable, "Chat store unavailable")));

        var response = await _client.DeleteAsync("/api/v1/chat/history");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Chat store unavailable"));
    }

    [Test]
    [TestCase("POST", "/api/v1/chat/stream")]
    [TestCase("GET", "/api/v1/chat/history")]
    [TestCase("DELETE", "/api/v1/chat/history")]
    public async Task Endpoints_WhenNoAuthHeader_Return401(string method, string route)
    {
        using var anonymousClient = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), route);

        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = JsonContent.Create(new SendMessageRequest("hello"));
        }

        var response = await anonymousClient.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
