using Microsoft.Extensions.Caching.Distributed;

namespace BookTracker.Api.Security;

/// <summary>Per-user daily token spend, tracked in IDistributedCache (resets after 24h).</summary>
public static class TokenBudget
{
    private static string Key(string user)
        => $"budget:{user}:{DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}";

    public static async Task<int> GetAsync(IDistributedCache cache, string user, CancellationToken ct)
    {
        var value = await cache.GetStringAsync(Key(user), ct);
        return int.TryParse(value, out var spent) ? spent : 0;
    }

    public static async Task AddAsync(IDistributedCache cache, string user, int tokens, CancellationToken ct)
    {
        var spent = await GetAsync(cache, user, ct);
        await cache.SetStringAsync(
            Key(user),
            (spent + tokens).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) },
            ct);
    }
}
