namespace Shared.Api.Responses;

public class IdempotencyResponse
{
    public int StatusCode { get; set; }
    
    public string Body { get; set; } = string.Empty;
    
    public Dictionary<string, string> Headers { get; set; } = [];
}