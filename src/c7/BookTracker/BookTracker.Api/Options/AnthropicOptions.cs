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
}
