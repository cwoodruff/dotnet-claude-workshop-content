namespace BookTracker.Api.Options;

/// <summary>
/// Bound from the "Anthropic" configuration section. ApiKey comes from user-secrets (never committed);
/// Model/MaxTokens/SystemPrompt come from appsettings.json.
/// </summary>
public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-4-6";

    public int MaxTokens { get; set; } = 1024;

    public string SystemPrompt { get; set; } = string.Empty;
}
