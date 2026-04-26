namespace Shared.Caching.Models;

public static class CacheKeys
{
    public static string UserById(Guid userId)
        => $"users:{userId}";

    public static string ProductById(Guid productId)
        => $"products:{productId}";
}