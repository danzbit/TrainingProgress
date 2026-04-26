namespace Shared.Kernal.Errors;

public sealed record Error(ErrorCode Code, string? Description = null)
{
    public static readonly Error None = new(ErrorCode.None);
    
    public static readonly Error UnexpectedError = new(ErrorCode.UnexpectedError);
    
    public static readonly Error EntityNotFound = new(ErrorCode.EntityNotFound, "Entity not found.");
    
    public static readonly Error EntityAlreadyExists = new(ErrorCode.EntityAlreadyExists, "Entity already exists.");
}