namespace Shared.Kernal.Errors;

public enum ErrorCode
{
    /// <summary>
    /// No error occurred. The operation completed successfully.
    /// </summary>
    None = 0,

    /// <summary>
    /// An unexpected or unhandled error occurred during execution.
    /// This usually indicates a server-side failure.
    /// </summary>
    UnexpectedError,

    /// <summary>
    /// The requested entity does not exist or is not accessible
    /// in the current security context.
    /// </summary>
    EntityNotFound,

    /// <summary>
    /// An entity with the same unique identifier or constrained
    /// property already exists.
    /// </summary>
    EntityAlreadyExists,

    /// <summary>
    /// The request payload could not be deserialized into
    /// the expected format or schema.
    /// </summary>
    DeserializationFailed,

    /// <summary>
    /// The request failed validation checks, such as missing required
    /// </summary>
    ValidationFailed,

    /// <summary>
    /// A required step in a saga workflow failed, causing the entire saga to be marked as failed.
    /// </summary>
    SagaStepFailed,

    /// <summary>
    /// A downstream dependency is unavailable or failed to respond successfully.
    /// </summary>
    DownstreamServiceUnavailable,

    /// <summary>
    /// The user attempted to perform an action that is not allowed due to business rules or constraints.
    /// </summary>
    UserAlreadyExist,

    /// <summary>
    /// The specified user was not found in the system.
    /// </summary>
    UserNotFound,

    /// <summary> 
    /// The user is not authorized to perform the requested action.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// An error occurred while creating a new user account.
    /// </summary>
    UserCreation,

    /// <summary>
    /// The refresh token is missing or empty.
    /// </summary>
    RefreshTokenRequired,

    /// <summary>
    /// The refresh token is invalid or has expired.
    /// </summary>
    InvalidRefreshToken,

    /// <summary>
    /// The token claims are invalid or malformed.
    /// </summary>
    InvalidTokenClaims
}
