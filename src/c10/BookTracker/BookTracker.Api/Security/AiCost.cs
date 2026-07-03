namespace BookTracker.Api.Security;

/// <summary>
/// Estimates the USD cost of a call from token usage and model. Rates are per 1M tokens (input/output)
/// — keep in sync with the published pricing. Used by the audit log.
/// </summary>
public static class AiCost
{
    public static decimal Estimate(string model, int inputTokens, int outputTokens)
    {
        var (inRate, outRate) = Rates(model);
        return (inputTokens / 1_000_000m * inRate) + (outputTokens / 1_000_000m * outRate);
    }

    // (input $/1M, output $/1M)
    private static (decimal In, decimal Out) Rates(string model)
    {
        var m = model.ToLowerInvariant();
        if (m.Contains("haiku")) return (1m, 5m);
        if (m.Contains("sonnet")) return (3m, 15m);
        if (m.Contains("opus")) return (5m, 25m);
        if (m.Contains("fable")) return (10m, 50m);
        return (3m, 15m); // default to Sonnet-tier
    }
}
