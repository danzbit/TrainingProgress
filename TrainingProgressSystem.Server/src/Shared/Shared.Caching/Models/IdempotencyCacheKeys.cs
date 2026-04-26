namespace Shared.Caching.Models;

public static class IdempotencyCacheKeys
{
    private const string Prefix = "idempotency";
    
    public static string Response(string method, string path, string idempotencyKey)
        => $"{Prefix}:response:{method.ToUpperInvariant()}:{path}:{idempotencyKey}";
}