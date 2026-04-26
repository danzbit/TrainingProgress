using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Abstractions.Idempotency;
using Shared.Api.Middlewares;
using Shared.Api.Responses;
using Shared.Kernal.Headers;

namespace Shared.Api.Tests.Middlewares;

[TestFixture]
public class IdempotencyMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_BypassesPipelineForNonIdempotentMethods()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var service = new Mock<IIdempotencyService>(MockBehavior.Strict);
        var context = CreateHttpContext("GET", "/api/workouts");

        var sut = new IdempotencyMiddleware(next);

        await sut.InvokeAsync(context, service.Object);

        Assert.That(nextCalled, Is.True);
        Assert.That(context.Response.Headers.ContainsKey(IdempotencyHeaders.IdempotencyKey), Is.False);
    }

    [Test]
    public async Task InvokeAsync_ReturnsCachedResponse_WhenEntryExists()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var cachedResponse = new IdempotencyResponse
        {
            StatusCode = 200,
            Body = "cached-body",
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["X-From-Cache"] = "true"
            }
        };

        var service = new Mock<IIdempotencyService>(MockBehavior.Strict);
        service.Setup(s => s.GetResponseAsync("POST", "/api/workouts", "idem-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        var context = CreateHttpContext("POST", "/api/workouts");
        context.Request.Headers[IdempotencyHeaders.IdempotencyKey] = "idem-123";

        var sut = new IdempotencyMiddleware(next);

        await sut.InvokeAsync(context, service.Object);

        Assert.That(nextCalled, Is.False);
        Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        Assert.That(context.Response.Headers["X-From-Cache"].ToString(), Is.EqualTo("true"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(body, Is.EqualTo("cached-body"));
    }

    [Test]
    public async Task InvokeAsync_ExecutesNextAndSavesResponse_WhenCacheMisses()
    {
        var service = new Mock<IIdempotencyService>(MockBehavior.Strict);
        service.Setup(s => s.GetResponseAsync("POST", "/api/progress", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyResponse?)null);

        string? capturedKey = null;
        IdempotencyResponse? savedResponse = null;
        service.Setup(s => s.SaveResponseAsync("POST", "/api/progress", It.IsAny<string>(), It.IsAny<IdempotencyResponse>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, IdempotencyResponse, CancellationToken>((_, _, key, response, _) =>
            {
                capturedKey = key;
                savedResponse = response;
            })
            .Returns(Task.CompletedTask);

        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status201Created;
            ctx.Response.Headers["Transfer-Encoding"] = "chunked";
            ctx.Response.Headers["X-Source"] = "api";
            await ctx.Response.WriteAsync("created");
        };

        var context = CreateHttpContext("POST", "/api/progress");
        var sut = new IdempotencyMiddleware(next);

        await sut.InvokeAsync(context, service.Object);

        Assert.That(capturedKey, Is.Not.Null.And.Not.Empty);
        Assert.That(context.Request.Headers[IdempotencyHeaders.IdempotencyKey].ToString(), Is.EqualTo(capturedKey));
        Assert.That(context.Response.Headers[IdempotencyHeaders.IdempotencyKey].ToString(), Is.EqualTo(capturedKey));

        Assert.That(savedResponse, Is.Not.Null);
        Assert.That(savedResponse!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(savedResponse.Body, Is.EqualTo("created"));
        Assert.That(savedResponse.Headers.ContainsKey("Transfer-Encoding"), Is.False);
        Assert.That(savedResponse.Headers["X-Source"], Is.EqualTo("api"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(body, Is.EqualTo("created"));
    }

    [Test]
    public async Task InvokeAsync_BypassesPipelineForGrpcRequests()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var service = new Mock<IIdempotencyService>(MockBehavior.Strict);
        var context = CreateHttpContext("POST", "/grpc.test.Service/Method", "application/grpc");

        var sut = new IdempotencyMiddleware(next);

        await sut.InvokeAsync(context, service.Object);

        Assert.That(nextCalled, Is.True);
        service.VerifyNoOtherCalls();
    }

    private static DefaultHttpContext CreateHttpContext(string method, string path, string? contentType = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.ContentType = contentType;
        context.Response.Body = new MemoryStream();
        return context;
    }
}