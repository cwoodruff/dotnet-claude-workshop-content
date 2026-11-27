using Anthropic.Models.Messages;

namespace BookTracker.Api.Options;

/// <summary>
/// Bound from the "Anthropic" configuration section. ApiKey comes from user-secrets (never committed);
/// Model/MaxTokens/SystemPrompt come from appsettings.json.
/// </summary>
public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-opus-5-5";

    public int MaxTokens { get; set; } = 4096;

    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// How much the model thinks before it answers: low, medium, high, xhigh or max. Null or empty
    /// leaves the model's default (medium on Opus 5.5). MaxTokens caps the bill per call; effort decides
    /// how much of that budget goes to thinking, which bills as output.
    /// </summary>
    public string? Effort { get; set; }

    /// <summary>The OutputConfig to send with every request, or null when Effort is not set.</summary>
    public OutputConfig? ToOutputConfig()
        => string.IsNullOrWhiteSpace(Effort) ? null : new OutputConfig { Effort = Effort };

    /// <summary>Per-user daily token budget (input + output) enforced by TokenBudgetMiddleware.</summary>
    public int DailyTokenCap { get; set; } = 200_000;

    /// <summary>
    /// Lab 2 Part C: true runs /api/agent through the SDK's tool runner (ToolRunnerAgentService),
    /// false through the hand-written loop (AgentService).
    /// </summary>
    public bool UseToolRunner { get; set; }
}
