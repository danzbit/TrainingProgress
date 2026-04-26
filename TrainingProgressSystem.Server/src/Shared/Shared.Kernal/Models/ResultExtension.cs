using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Microsoft.AspNetCore.Http;

namespace Shared.Kernal.Models;

public static class ResultExtension
{
    public static IActionResult ToActionResult(this Result result)
    {
        return !result.IsFailure ? new OkResult() : CreateErrorResponse(result.Error);
    }

    public static IActionResult ToActionResult<T>(this ResultOfT<T> result)
    {
        return !result.IsFailure ? new OkObjectResult(result.Value) : CreateErrorResponse(result.Error, result.Value);
    }

    public static IActionResult ToActionResult<T, U>(this ResultOfT<T> result, U successResponse)
    {
        return !result.IsFailure ? new OkObjectResult(successResponse) : CreateErrorResponse(result.Error, result.Value);
    }

    private static IActionResult CreateErrorResponse<T>(Error error, T value)
    {
        if (value is IEnumerable<string> validationMessages)
        {
            var messages = validationMessages
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToArray();

            if (messages.Length > 0)
            {
                var problemDetails = new
                {
                    title = "One or more validation errors occurred.",
                    status = StatusCodes.Status400BadRequest,
                    detail = error.Description,
                    errors = new Dictionary<string, string[]>
                    {
                        ["errors"] = messages
                    }
                };

                return new BadRequestObjectResult(problemDetails);
            }
        }

        return CreateErrorResponse(error);
    }

    private static IActionResult CreateErrorResponse(Error error)
    {
        var message = new { message = error.Description };
        return error.Code switch
        {
            ErrorCode.None => new BadRequestResult(),
            ErrorCode.UnexpectedError => new BadRequestObjectResult(message),
            ErrorCode.EntityNotFound => new NotFoundObjectResult(message),
            ErrorCode.EntityAlreadyExists => new BadRequestObjectResult(message),
            ErrorCode.DeserializationFailed => new BadRequestObjectResult(message),
            ErrorCode.ValidationFailed => new BadRequestObjectResult(message),
            ErrorCode.SagaStepFailed => new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            },
            ErrorCode.DownstreamServiceUnavailable => new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            },
            _ => new BadRequestObjectResult(message)
        };
    }
}