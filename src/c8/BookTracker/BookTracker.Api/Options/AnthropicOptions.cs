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
    /// Lab 2 Part C: true runs /api/agent through the SDK's tool runner (ToolRunnerAgentService),
    /// false through the hand-written loop (AgentService).
    /// </summary>
    public bool UseToolRunner { get; set; }
}
