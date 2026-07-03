using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Api.Security;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// Wraps the official Anthropic C# SDK. Maps stored history to the SDK message list, appends the new
/// user turn, sends a cached system prompt, and extracts the reply text + usage.
/// </summary>
public class ClaudeService : IClaudeService
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;

    public ClaudeService(AnthropicClient client, IOptions<AnthropicOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<ChatResponse> ChatAsync(
        string userMessage,
        IReadOnlyList<ChatTurn> history,
        CancellationToken ct = default)
    {
        // Structural separation: user text is wrapped in <user_input> in the USER turn (never the
        // system prompt), and the system prompt tells the model to treat it as data, not instructions.
        var messages = history
            .Select(t => new MessageParam
            {
                Role = t.Role == "assistant" ? Role.Assistant : Role.User,
                Content = t.Role == "assistant" ? t.Content : PromptSafety.Wrap(t.Content),
            })
            .Append(new MessageParam { Role = Role.User, Content = PromptSafety.Wrap(userMessage) })
            .ToList();

        // The Anthropic API is stateless — the full history is sent every call. The system prompt is
        // marked ephemeral so the (large) static prefix bills at ~10% on repeat calls.
        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = _options.Model,
            MaxTokens = _options.MaxTokens,
            System = new List<TextBlockParam>
            {
                new() { Text = _options.SystemPrompt, CacheControl = new CacheControlEphemeral() },
            },
            Messages = messages,
        }, cancellationToken: ct);

        var reply = string.Concat(
            response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

        return new ChatResponse(
            reply,
            (int)response.Usage.InputTokens,
            (int)response.Usage.OutputTokens,
            (int)(response.Usage.CacheReadInputTokens ?? 0));
    }
}
