using System.Text.Json;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Api.Middlewares;

namespace Shared.Api.Tests.Middlewares;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    [Test]
    public async Task TryHandleAsync_ReturnsMappedProblemDetails_ForArgumentException()
    {
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(logger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/workouts";
        context.Request.Method = HttpMethods.Post;
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("Invalid payload", "duration");

        var handled = await sut.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/problem+json"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(400));
        Assert.That(root.GetProperty("title").GetString(), Is.EqualTo("Invalid Argument"));
        Assert.That(root.GetProperty("detail").GetString(), Is.EqualTo("Invalid payload (Parameter 'duration')"));
        Assert.That(root.GetProperty("instance").GetString(), Is.EqualTo("/api/workouts"));
        Assert.That(root.GetProperty("paramName").GetString(), Is.EqualTo("duration"));
    }

    [Test]
    public async Task TryHandleAsync_ReturnsGenericProblemDetails_ForUnhandledExceptionType()
    {
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(logger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/goals";
        context.Request.Method = HttpMethods.Get;
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Internal details should not leak");

        var handled = await sut.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(500));
        Assert.That(root.GetProperty("title").GetString(), Is.EqualTo("Internal Server Error"));
        Assert.That(root.GetProperty("detail").GetString(), Is.EqualTo("An unexpected error occurred. Please contact support."));
        Assert.That(root.GetProperty("instance").GetString(), Is.EqualTo("/api/goals"));
    }
}