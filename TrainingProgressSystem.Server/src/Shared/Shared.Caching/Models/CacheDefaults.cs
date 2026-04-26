namespace Shared.Caching.Models;

public static class CacheDefaults
{
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);
    
    public static readonly TimeSpan DefaultSlidingTtl = TimeSpan.FromMinutes(5);
}