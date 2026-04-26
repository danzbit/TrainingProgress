using Shared.Kernal.Errors;

namespace Shared.Kernal.Results;

public class Result
{
    private bool IsSuccess { get; }
    
    public Error Error { get; }

    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("Successful result cannot have an error message.");
            case false when error == Error.None:
                throw new InvalidOperationException("Failure result must have an error message.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}