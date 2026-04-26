using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;

namespace Shared.Api.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private static readonly Dictionary<Type, (int StatusCode, string Title, string Type)> ExceptionMapping = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status400BadRequest, "Bad Request", "https://httpwg.org/specs/rfc7231.html#status.400") },
        { typeof(InvalidOperationException), (StatusCodes.Status400BadRequest, "Invalid Operation", "https://httpwg.org/specs/rfc7231.html#status.400") },
        { typeof(ArgumentException), (StatusCodes.Status400BadRequest, "Invalid Argument", "https://httpwg.org/specs/rfc7231.html#status.400") },
        { typeof(KeyNotFoundException), (StatusCodes.Status404NotFound, "Not Found", "https://httpwg.org/specs/rfc7231.html#status.404") },
        { typeof(UnauthorizedAccessException), (StatusCodes.Status403Forbidden, "Forbidden", "https://httpwg.org/specs/rfc7231.html#status.403") }
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception at {Path} {Method}", context.Request.Path, context.Request.Method);

        var exceptionType = exception.GetType();
        var (statusCode, title, typeUri) = ExceptionMapping.TryGetValue(exceptionType, out var mapping)
            ? mapping
            : (StatusCodes.Status500InternalServerError, "Internal Server Error", "https://httpwg.org/specs/rfc7231.html#status.500");

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = typeUri,
            Detail = statusCode == StatusCodes.Status500InternalServerError 
                ? "An unexpected error occurred. Please contact support."
                : exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ArgumentException argEx && !string.IsNullOrEmpty(argEx.ParamName))
        {
            problemDetails.Extensions["paramName"] = argEx.ParamName;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, _options), cancellationToken: cancellationToken);
         
         return true;
    }
}
