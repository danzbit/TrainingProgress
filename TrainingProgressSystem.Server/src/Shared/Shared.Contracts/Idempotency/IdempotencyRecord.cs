namespace Shared.Contracts.Idempotency;

public record IdempotencyRecord(
    string IdempotencyKey, 
    string Method, 
    string Path, 
    int StatusCode, 
    string ResponseBody, 
    Dictionary<string, string> Headers, 
    DateTime CreatedAt);