using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Api.Tools;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// The Anthropic SDK implementation of the agent LLM seam. All SDK-specific message/block handling
/// lives here, so AgentService's loop stays provider-neutral and testable.
/// </summary>
public class AnthropicAgentLlm : IAgentLlm
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;

    public AnthropicAgentLlm(AnthropicClient client, IOptions<AnthropicOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public IAgentConversation StartConversation(string userMessage)
        => new AnthropicAgentConversation(_client, _options, userMessage);

    /// <summary>
    /// Turns a response's content into the assistant turn to send back with tool results. Thinking,
    /// text and tool_use blocks are kept in their original order; dropping a thinking block makes the
    /// next call fail.
    /// </summary>
    public static List<ContentBlockParam> ToAssistantContent(IReadOnlyList<ContentBlock> content)
    {
        var assistantContent = new List<ContentBlockParam>();
        foreach (var block in content)
        {
            if (block.TryPickText(out TextBlock? text))
            {
                assistantContent.Add(new TextBlockParam { Text = text.Text });
            }
            else if (block.TryPickThinking(out ThinkingBlock? thinking))
            {
                // Thinking is on by default on current models. When a tool result follows, the API
                // expects the thinking blocks back unchanged, signature included, in the same order.
                assistantContent.Add(new ThinkingBlockParam
                {
                    Thinking = thinking.Thinking,
                    Signature = thinking.Signature,
                });
            }
            else if (block.TryPickRedactedThinking(out RedactedThinkingBlock? redacted))
            {
                assistantContent.Add(new RedactedThinkingBlockParam(redacted.Data));
            }
            else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
            {
                assistantContent.Add(new ToolUseBlockParam
                {
                    ID = toolUse.ID,
                    Name = toolUse.Name,
                    Input = toolUse.Input,
                });
            }
        }

        return assistantContent;
    }

    private const string MaxTokensMessage =
        "Stopped: the reply hit the MaxTokens limit before it finished. Raise Anthropic:MaxTokens.";

    private sealed class AnthropicAgentConversation : IAgentConversation
    {
        private readonly AnthropicClient _client;
        private readonly AnthropicOptions _options;
        private readonly List<MessageParam> _messages;
        private Message? _lastResponse;

        public AnthropicAgentConversation(AnthropicClient client, AnthropicOptions options, string userMessage)
        {
            _client = client;
            _options = options;
            _messages = [new UserMessageParam(userMessage)];
        }

        public Task<AgentTurn> SendUserMessageAsync(CancellationToken ct = default) => CreateAsync(ct);

        public Task<AgentTurn> SubmitToolOutcomesAsync(
            IReadOnlyList<AgentToolOutcome> outcomes,
            CancellationToken ct = default)
        {
            // Echo the assistant's previous turn (thinking, text and tool_use blocks, in order) ...
            _messages.Add(new AssistantMessageParam(ToAssistantContent(_lastResponse!.Content)));

            // ... then the matching tool_result blocks as the next user turn.
            var toolResults = outcomes
                .Select(o => (ContentBlockParam)new ToolResultBlockParam
                {
                    ToolUseID = o.Id,
                    Content = o.Result,
                    IsError = o.IsError,
                })
                .ToList();
            _messages.Add(new UserMessageParam(toolResults));

            return CreateAsync(ct);
        }

        private async Task<AgentTurn> CreateAsync(CancellationToken ct)
        {
            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = new List<TextBlockParam>
                {
                    new() { Text = _options.SystemPrompt, CacheControl = new CacheControlEphemeral() },
                },
                Messages = _messages,
                Tools = BookTrackerTools.Definitions.Select(t => (ToolUnion)t).ToList(),
            }, cancellationToken: ct);

            _lastResponse = response;

            if (response.StopReason == "max_tokens")
            {
                // MaxTokens covers thinking plus the reply. Hitting it mid-turn is a config problem,
                // not an answer, so say so instead of returning a cut-off reply.
                return new AgentTurn(MaxTokensMessage, []);
            }

            var finalText = string.Concat(
                response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

            var toolRequests = new List<AgentToolRequest>();
            foreach (var block in response.Content)
            {
                if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                {
                    toolRequests.Add(new AgentToolRequest(toolUse.ID, toolUse.Name, toolUse.Input));
                }
            }

            return new AgentTurn(finalText, toolRequests);
        }
    }
}
