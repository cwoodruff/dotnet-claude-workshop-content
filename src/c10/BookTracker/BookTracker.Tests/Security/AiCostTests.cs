using BookTracker.Api.Security;

namespace BookTracker.Tests.Security;

public class AiCostTests
{
    [Fact]
    public void Estimate_UsesPerModelRates()
    {
        // Sonnet: $3/1M input, $15/1M output → 1M in + 1M out = $18.
        var cost = AiCost.Estimate("claude-sonnet-4-6", 1_000_000, 1_000_000);
        Assert.Equal(18m, cost);
    }

    [Fact]
    public void Estimate_HaikuIsCheaperThanOpus()
    {
        var haiku = AiCost.Estimate("claude-haiku-4-5", 100_000, 100_000);
        var opus = AiCost.Estimate("claude-opus-4-8", 100_000, 100_000);
        Assert.True(haiku < opus);
    }

    [Fact]
    public void Estimate_ZeroTokens_IsZero()
        => Assert.Equal(0m, AiCost.Estimate("claude-sonnet-4-6", 0, 0));
}
