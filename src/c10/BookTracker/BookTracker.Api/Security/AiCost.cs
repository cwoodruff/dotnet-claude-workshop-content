namespace BookTracker.Api.Security;

/// <summary>
/// Estimates the USD cost of a call from token usage and model. Used by the audit log.
/// Rates are per 1M tokens (input/output) for the Claude 5.5 lineup, from the Claude API pricing
/// page, checked 2026-11-27. It's an estimate: older models (Opus 4.x, Sonnet 4.x, Haiku 4.5) cost
/// more, cached input is billed lower, and Haiku 5.5's higher rate for prompts over 100k tokens is
/// left out because the audit log sums usage across an agent's turns, not per request.
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
        if (m.Contains("opus")) return (4m, 20m);
        if (m.Contains("sonnet")) return (2m, 10m);
        if (m.Contains("haiku")) return (0.10m, 0.50m);
        if (m.Contains("fable")) return (8m, 40m);

        // Unknown model: price it as Opus, the app's default, so the estimate errs high.
        return (4m, 20m);
    }
}
