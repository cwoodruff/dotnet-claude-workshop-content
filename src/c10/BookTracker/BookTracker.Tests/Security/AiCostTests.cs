using BookTracker.Api.Security;

namespace BookTracker.Tests.Security;

public class AiCostTests
{
    [Fact]
    public void Estimate_Opus_UsesOpusRates()
    {
        // Opus 5.5: $4/1M input, $20/1M output → 1M in + 1M out = $24.
        var cost = AiCost.Estimate("claude-opus-5-5", 1_000_000, 1_000_000);
        Assert.Equal(24m, cost);
    }

    [Fact]
    public void Estimate_Sonnet_UsesSonnetRates()
    {
        // Sonnet 5.5: $2/1M input, $10/1M output → $12.
        var cost = AiCost.Estimate("claude-sonnet-5-5", 1_000_000, 1_000_000);
        Assert.Equal(12m, cost);
    }

    [Fact]
    public void Estimate_Haiku_PriceStepsUpForLongPrompts()
    {
        // Up to 100k input tokens: $0.10 / $0.50. Above that: $0.50 / $2.50.
        Assert.Equal(0.0015m, AiCost.Estimate("claude-haiku-5-5", 10_000, 1_000));
        Assert.Equal(0.1025m, AiCost.Estimate("claude-haiku-5-5", 200_000, 1_000));
    }

    [Fact]
    public void Estimate_UnknownModel_PricesAsOpus()
        => Assert.Equal(
            AiCost.Estimate("claude-opus-5-5", 50_000, 2_000),
            AiCost.Estimate("some-future-model", 50_000, 2_000));

    [Fact]
    public void Estimate_ZeroTokens_IsZero()
        => Assert.Equal(0m, AiCost.Estimate("claude-opus-5-5", 0, 0));
}
