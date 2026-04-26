using System.Net;
using System.Text;
using AiChatService.Domain.Interfaces;
using AiChatService.Infrastructure.Llm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiChatService.Infrastructure.Tests.Llm;

[TestFixture]
public class GroqLlmClientTests
{
    [Test]
    public async Task StreamCompletionAsync_WhenResponseIsSuccessful_YieldsParsedContentChunks()
    {
        var responseText = string.Join("\n", new[]
        {
            "data: {\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}",
            string.Empty,
            "data: {\"choices\":[{\"delta\":{}}]}",
            "data: invalid-json",
            "data: {\"choices\":[{\"delta\":{\"content\":\" world\"}}]}",
            "data: [DONE]"
        });

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseText, Encoding.UTF8, "text/event-stream")
        });
        var client = CreateClient(handler);

        var chunks = await ToListAsync(client.StreamCompletionAsync(
            [("system", "prompt"), ("user", "hello")],
            CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "Hello", " world" }));
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
        Assert.That(handler.Requests[0].Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(handler.Requests[0].RequestUri, Is.EqualTo(new Uri("https://api.groq.com/openai/v1/chat/completions")));
    }

    [Test]
    public async Task StreamCompletionAsync_WhenApiReturnsError_YieldsServiceErrorMessage()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("rate limited", Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        var chunks = await ToListAsync(client.StreamCompletionAsync([("user", "hello")], CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "[AI service error: 429]" }));
    }

    [Test]
    public async Task StreamCompletionAsync_WhenHttpRequestFails_YieldsUnavailableMessage()
    {
        var handler = new StubHttpMessageHandler(_ => throw new HttpRequestException("network down"));
        var client = CreateClient(handler);

        var chunks = await ToListAsync(client.StreamCompletionAsync([("user", "hello")], CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "[AI service is temporarily unavailable. Please try again in a moment.]" }));
    }

    [Test]
    public async Task StreamCompletionAsync_WhenTimeoutOccurs_YieldsTimeoutMessage()
    {
        var handler = new StubHttpMessageHandler(_ => throw new TaskCanceledException("timeout"));
        var client = CreateClient(handler);

        var chunks = await ToListAsync(client.StreamCompletionAsync([("user", "hello")], CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "[AI service timed out. Please try again.]" }));
    }

    private static GroqLlmClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.groq.com")
        };
        var httpClientFactory = new StubHttpClientFactory(httpClient);

        return new GroqLlmClient(
            httpClientFactory,
            LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning)).CreateLogger<GroqLlmClient>(),
            Options.Create(new GroqOptions { Model = "test-model", ApiKey = "test-api-key" }));
    }

    private static async Task<List<string>> ToListAsync(IAsyncEnumerable<string> source)
    {
        var items = new List<string>();
        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));
            return Task.FromResult(responseFactory(request));
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            return new HttpRequestMessage(request.Method, request.RequestUri);
        }
    }
}
