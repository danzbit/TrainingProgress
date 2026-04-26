using Shared.Kernal.Errors;

namespace Shared.Kernal.Results;

public class ResultOfT<T> : Result
{
    public T Value = default!;

    private ResultOfT() : base(true, Error.None)
    {
        
    }
    
    private ResultOfT(bool isSuccess) : base(isSuccess, Error.None)
    {
        Value = default!;
    }
    
    private ResultOfT(T value) : base(true, Error.None)
    {
        Value = value;
    }

    private ResultOfT(Error error) : base(false, error)
    {
        Value = default!;
    }
    
    private ResultOfT(T value, Error error) : base(false, error)
    {
        Value = value;
    }

    public new static ResultOfT<T> Success() => new();
    
    public static ResultOfT<T> Success(T value) => new(value);
    
    public static ResultOfT<T> Failure() => new(false);

    public new static ResultOfT<T> Failure(Error error) => new(error);
    
    public static ResultOfT<T> Failure(T value, Error error) => new(value, error);
}