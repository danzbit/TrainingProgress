using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Shared.Api.Responses;
using Shared.Abstractions.Caching;
using Shared.Abstractions.Idempotency;
using Shared.Kernal.Headers;

namespace Shared.Api.Middlewares;

public class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
    {
        if (!IsIdempotentMethod(context.Request.Method) || IsGrpcRequest(context))
        {
            await next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers.TryGetValue(IdempotencyHeaders.IdempotencyKey, out var idempotencyKeyHeader)
            && !string.IsNullOrWhiteSpace(idempotencyKeyHeader)
            ? idempotencyKeyHeader.ToString().Trim()
            : Guid.NewGuid().ToString("N");

        context.Request.Headers[IdempotencyHeaders.IdempotencyKey] = idempotencyKey;
        context.Response.Headers[IdempotencyHeaders.IdempotencyKey] = idempotencyKey;

        var cachedResponse = await idempotencyService.GetResponseAsync(
            context.Request.Method,
            context.Request.Path,
            idempotencyKey);

        if (cachedResponse != null)
        {
            context.Response.StatusCode = cachedResponse.StatusCode;
            foreach (var header in cachedResponse.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }

            if (!string.IsNullOrWhiteSpace(cachedResponse.Body))
            {
                await context.Response.WriteAsync(cachedResponse.Body);
            }

            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            var responseToCache = new IdempotencyResponse
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBody,
                Headers = context.Response.Headers
                    .Where(h => !h.Key.StartsWith("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            await idempotencyService.SaveResponseAsync(
                context.Request.Method,
                context.Request.Path,
                idempotencyKey,
                responseToCache);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static bool IsIdempotentMethod(string method) =>
        method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
        method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
        method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);

    private static bool IsGrpcRequest(HttpContext context) =>
        context.Request.ContentType?.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase) ?? false;
}