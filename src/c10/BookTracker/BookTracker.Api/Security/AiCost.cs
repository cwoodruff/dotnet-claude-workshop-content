namespace BookTracker.Api.Security;

/// <summary>
/// Estimates the USD cost of a call from token usage and model. Used by the audit log.
/// Rates are per 1M tokens (input/output) for the Claude 5.5 lineup, from the Claude API pricing
/// page, checked 2026-11-27. Older models (Opus 4.x, Sonnet 4.x, Haiku 4.5) cost more than these,
/// and cached input is billed lower, so treat the result as an estimate.
/// </summary>
public static class AiCost
{
    // Haiku 5.5's rates step up for prompts over this many input tokens.
    private const int HaikuLongPromptThreshold = 100_000;

    public static decimal Estimate(string model, int inputTokens, int outputTokens)
    {
        var (inRate, outRate) = Rates(model, inputTokens);
        return (inputTokens / 1_000_000m * inRate) + (outputTokens / 1_000_000m * outRate);
    }

    // (input $/1M, output $/1M)
    private static (decimal In, decimal Out) Rates(string model, int inputTokens)
    {
        var m = model.ToLowerInvariant();
        if (m.Contains("opus")) return (4m, 20m);
        if (m.Contains("sonnet")) return (2m, 10m);
        if (m.Contains("haiku"))
        {
            return inputTokens > HaikuLongPromptThreshold ? (0.50m, 2.50m) : (0.10m, 0.50m);
        }

        if (m.Contains("fable")) return (8m, 40m);

        // Unknown model: price it as Opus, the app's default, so the estimate errs high.
        return (4m, 20m);
    }
}
